#include"stdafx.h"
#include"crashNativeHandler.h"
#include"stackWalker.h"

using namespace ao;

namespace
{
	std::wstring StringToWString(std::string src)
	{
		// 変換後のサイズ計算
		u32 size = MultiByteToWideChar(CP_UTF8, 0, src.c_str(), -1, nullptr, 0);

		// 変換先のバッファ確保
		auto dest = (wchar_t*)alloca(size * sizeof(wchar_t));

		// string -> wstring
		MultiByteToWideChar(CP_UTF8, 0, src.c_str(), -1, dest, size);

		// wstringの生成
		return std::wstring(dest, dest + size - 1);
	}

	StackWalker stackWalker;

	std::wstring crashAppName;
}

void CrashNativeHandler::setup(const std::wstring& appName)
{
	stackWalker.initialize();
	SetUnhandledExceptionFilter(dump);

	crashAppName = appName;
}

void CrashNativeHandler::terminate()
{
	stackWalker.finalize();
}

LONG WINAPI CrashNativeHandler::dump(EXCEPTION_POINTERS* exp)
{
	SYSTEMTIME time;
	GetLocalTime(&time);

	auto process = GetCurrentProcess();
	auto processId = GetCurrentProcessId();
	auto threadId = GetCurrentThreadId();

	wchar_t szFileName[MAX_PATH];
	StringCchPrintf(szFileName, MAX_PATH, L"%s-%04d-%02d%02d-%02d%02d.dmp", crashAppName.c_str(), time.wYear, time.wMonth, time.wDay, time.wHour, time.wMinute);

	HANDLE hDumpFile = CreateFile(szFileName, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_WRITE | FILE_SHARE_READ, 0, CREATE_ALWAYS, 0, 0);


	MINIDUMP_EXCEPTION_INFORMATION ExpParam;

	ExpParam.ThreadId = threadId;
	ExpParam.ExceptionPointers = exp;
	ExpParam.ClientPointers = TRUE;

	bool bMiniDumpSuccessful = MiniDumpWriteDump(GetCurrentProcess(), processId, hDumpFile, MiniDumpWithDataSegs, &ExpParam, nullptr, nullptr);

	stackWalker.collect();

	const u32 nStackFrame = stackWalker.getStackFrame();

	for (u32 i = 0; i < nStackFrame; ++i)
	{
		stackWalker.resolve(i);
	}

	const u32 dirPathLength = GetCurrentDirectory(0, nullptr);

	auto dirPath = (wchar_t*)alloca(dirPathLength * sizeof(wchar_t));
	GetCurrentDirectory(dirPathLength, dirPath);

	wchar_t dumpPath[MAX_PATH];
	StringCchPrintf(dumpPath, MAX_PATH, L"\"%s\\%s\"", dirPath, szFileName);

	// 何故か第二引数（コマンドライン引数）には先頭にスペースが必要。でないと先頭の引数が解釈されない。
	std::wstring args(L" ");
	args.append(dumpPath);

	for (u32 i = 0; i < nStackFrame; ++i)
	{
		const auto& stack = stackWalker.getCallStack(i);
		if (stack.isValid() == false)
		{
			continue;
		}

		args.append(L" \"");
		args.append(StringToWString(stack.getUndecoratedName()));
		args.append(L"\" \"");
		args.append(std::to_wstring(stack.getLineNumber()));
		args.append(L"\" \"");
		args.append(StringToWString(stack.getFileName()));
		args.append(L"\"");
	}

	STARTUPINFO startupInfo = { 0 };
	startupInfo.cb = sizeof(STARTUPINFOA);

	PROCESS_INFORMATION processInformation = { 0 };

	bool result = CreateProcess(L"CrashHandler.exe", (LPWSTR)args.c_str(), nullptr, nullptr, false, 0, nullptr, nullptr, &startupInfo, &processInformation);
	if (result == false)
	{
		OutputDebugString(L"Failed to create process.");
	}

	return EXCEPTION_EXECUTE_HANDLER;
}