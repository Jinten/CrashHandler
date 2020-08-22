#pragma once

#include<Windows.h>
#include<string>

namespace ao
{
	class CrashNativeHandler
	{
	public:
		static void setup(const std::wstring& appName, const std::wstring& handlerPath = L"");
		static void terminate();

	public:
		static LONG WINAPI dump(EXCEPTION_POINTERS* exp);
	};
}