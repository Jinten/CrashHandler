#pragma once

#include<Windows.h>
#include<dbghelp.h>
#include<string>
#include<array>
#include"typeAlias.h"

#pragma comment(lib,"Dbghelp.lib")

namespace ao
{
	/*!-----------------------------------------------------------
	//	@struct  SymbolBuffer
	//	@brief  SymbolBuffer
	------------------------------------------------------------*/
	class SymbolBuffer : public SYMBOL_INFO
	{
	public:
		SymbolBuffer()
		{
			MaxNameLen = MaxStringLength;
			SizeOfStruct = sizeof(SYMBOL_INFO);
		}

		~SymbolBuffer()
		{

		}

	private:
		static constexpr size_t MaxStringLength = 512;

	private:
		wchar_t buffer[MaxStringLength];
	};

	/*!-----------------------------------------------------------
	//	@struct  CallStack
	//	@brief  CallStack
	------------------------------------------------------------*/
	class CallStack
	{
	public:
		CallStack() {}
		~CallStack() {}

		bool isValid() const
		{
			return mIsValid;
		}

		void setAddr(void* addr)
		{
			mAddr = addr;
		}

		void* getAddr() const
		{
			return mAddr;
		}

		const SymbolBuffer& getSymbol() const
		{
			return mSymbol;
		}

		const std::string& getUndecoratedName() const
		{
			return mUndecoratedName;
		}

		u32 getLineNumber() const
		{
			return mLineNumber;
		}

		const std::string& getFileName() const
		{
			return mFileName;
		}

		bool resolve(HANDLE process)
		{
			DWORD64 disp = 0;
			if (SymFromAddr(process, (DWORD64)mAddr, &disp, (SYMBOL_INFO*)&mSymbol) == false)
			{
				mIsValid = false;
				return false;
			}

			IMAGEHLP_LINE line;
			line.SizeOfStruct = sizeof(IMAGEHLP_LINE);

			if (SymGetLineFromAddr(process, (DWORD64)mAddr, (PDWORD)&disp, &line) == false)
			{
				mIsValid = false;
				return false;
			}

			mUndecoratedName.resize(MAX_UNDECORATEDNAME_LENGTH);
			if (UnDecorateSymbolName(mSymbol.Name, (LPSTR)mUndecoratedName.data(), MAX_UNDECORATEDNAME_LENGTH, UNDNAME_COMPLETE) == false)
			{
				mIsValid = false;
				return false;
			}

			mIsValid = true;
			mLineNumber = line.LineNumber;
			mFileName.assign(line.FileName);

			return mIsValid;
		}

	private:
		static constexpr u32 MAX_UNDECORATEDNAME_LENGTH = 512;

	private:
		bool mIsValid = false;

		void* mAddr = nullptr;
		SymbolBuffer mSymbol;

		std::string mUndecoratedName;

		u32 mLineNumber = 0;
		std::string mFileName;
	};

	/*!-----------------------------------------------------------
	//	@struct  StackWalker
	//	@brief  StackWalker
	------------------------------------------------------------*/
	class StackWalker
	{
	public:
		StackWalker() {}
		~StackWalker() {}

		void initialize()
		{
			mProcess = GetCurrentProcess();
			SymSetOptions(SYMOPT_DEFERRED_LOADS | SYMOPT_LOAD_LINES | SYMOPT_PUBLICS_ONLY);
			if (SymInitialize(mProcess, NULL, TRUE))
			{
				mReady = true;
			}
		}

		void finalize()
		{
			if (mProcess)
			{
				SymCleanup(mProcess);
				mProcess = nullptr;
			}
		}

		u32 getStackFrame()const
		{
			return mStackFrame;
		}

		void collect()
		{
			if (mReady == false)
			{
				return;
			}
			void* addrTbl[MAX_STACK_DEPTH];
			mStackFrame = CaptureStackBackTrace(0, MAX_STACK_DEPTH, addrTbl, nullptr);

			for (u32 i = 2; i < mStackFrame; ++i)
			{
				mCallStackTbl[i].setAddr(addrTbl[i]);
			}
		}

		bool resolve(u32 index)
		{
			if (mReady == false)
			{
				return false;
			}
			return mCallStackTbl[index].resolve(mProcess);
		}

		const CallStack& getCallStack(u32 index) const
		{
			return mCallStackTbl[index];
		}

	private:
		static constexpr u32 MAX_STACK_DEPTH = 63;

	private:
		bool mReady = false;
		HANDLE mProcess = nullptr;

		u32 mStackFrame = 0;
		std::array<CallStack, MAX_STACK_DEPTH> mCallStackTbl;
	};
}
