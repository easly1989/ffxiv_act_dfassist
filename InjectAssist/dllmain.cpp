// dllmain.cpp : Defines the entry point for the DLL application.
#include "stdafx.h"

DWORD AttachFunction(HMODULE hModule);
DWORD DetachFunction();

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  fdwReason, LPVOID lpReserved)
{
	hModule;
    lpReserved;

    switch (fdwReason)
    {
		case DLL_PROCESS_ATTACH:
			DisableThreadLibraryCalls( hModule );
			CreateThread( NULL, NULL, ( LPTHREAD_START_ROUTINE )AttachFunction, hModule, NULL, NULL );
			break;
		case DLL_PROCESS_DETACH:
			DetachFunction();
			break;
		case DLL_THREAD_DETACH:
		case DLL_THREAD_ATTACH:
			break;
    }

    return TRUE;
}

DWORD AttachFunction( HMODULE hModule )
{   
	MessageBox(0, TEXT("Attach"), TEXT("Attached to dll"), MB_OK);
	FreeLibraryAndExitThread(hModule, NULL);
	return 1;
}

DWORD DetachFunction()
{
	MessageBox(0, TEXT("Detach"), TEXT("Detached from dll"), MB_OK);
	return 1;
}