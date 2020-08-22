#include"../CrashNativeHandler/crashNativeHandler.h"

#ifdef _DEBUG
#pragma comment(lib, "../bin/Debug/CrashNativeHandler.lib")
#endif
#ifdef NDEBUG
#pragma comment(lib, "../bin/Release/CrashNativeHandler.lib")
#endif

class TestReturn
{

};

class TestReturnA
{

};

class TestReturnB : public TestReturnA
{

};

class Invoker
{
public:
	void invoke()
	{
		invoke_private();
	}

private:
	void invoke_private()
	{
		invoke_static_private();
	}

	static void invoke_static_private()
	{
		int* pBadPtr = nullptr;
		*pBadPtr = 0;
	}
};

static void Func6()
{
	Invoker invoker;
	invoker.invoke();
}

TestReturnA* Func5()
{
	Func6();
	return nullptr;
}

const TestReturnB& Func4()
{
	Func5();
	return TestReturnB(); // it's test code. no problem.
}

int Func3()
{
	Func4();
	return 0;
}

char* Func2(const char* v = nullptr)
{
	Func3();
	return nullptr;
}

TestReturn Func1(const int v = 0)
{
	Func2();
	return TestReturn();
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