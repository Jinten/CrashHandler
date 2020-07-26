#include"../CrashNativeHandler/crashNativeHandler.h"


void Func4()
{
	int* pBadPtr = nullptr;
	*pBadPtr = 0;
}

void Func3()
{
	Func4();
}

void Func2()
{
	Func3();
}

void Func1()
{
	Func2();
}

void Func0()
{	
	Func1();
}

int main()
{
	ao::CrashNativeHandler::setup(L"TestCrashApp-v1.0");

	Func0();

	ao::CrashNativeHandler::terminate();

	return 0;
}