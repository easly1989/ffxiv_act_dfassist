#pragma once

#include "wintoastlib.h"

#define DLLEXPORT extern "C" __declspec(dllexport)

using namespace WinToastLib;

typedef void(*ToastEventCallback)(int messageCode);

enum Results {
	ToastClicked,					// user clicked on the toast
	ToastDismissed,					// user dismissed the toast
	ToastTimeOut,					// toast timed out
	ToastHided,						// application hid the toast
	ToastNotActivated,				// toast was not activated
	ToastFailed,					// toast failed
	SystemNotSupported,				// system does not support toasts
	UnhandledOption,				// unhandled option
	MultipleTextNotSupported,		// multiple texts were provided
	InitializationFailure,			// toast notification manager initialization failure
	ToastNotLaunched,				// toast could not be launched,
	AwaitingInteractions			// when the toast is shown, awaiting for user interaction, timeone or an eventual error
};

/// <summary>
///	Creates a WinToast using template <c>Text01</c>
/// <remarks>See https://github.com/mohabouje/WinToast for more informations!</remarks>
/// </summary>
/// <param name="appName">The application name</param>
/// <param name="appUserModelID">The application unique ID</param>
/// <param name="toastMessage">The message to show</param>
/// <param name="eventCallback">The callback to call to handle results and interactions with the created toast</param>
/// <param name="attribution">The sender of the message (it is shown at the end of the message, for example SMS or Twitter</param>
/// <param name="duration">Indicates the duration of the Toast visualization, <c>Default: 0</c>.
/// Can be: 
/// <c>System = 0</c>
/// <c>Short = 1</c>
/// <c>Long = 2</c></param>
/// <param name="audioFile">Indicates the audio file to play on Toast visualization, <c>Default: 0</c>.
/// Can be:
/// <c>Default = 0</c>
/// <c>Silent = 1</c>
/// <c>Loop = 2</c></param>
/// <c>DefaultSound = 0</c>
/// <c>IM = 1</c> 
/// <c>Mail = 2</c>
/// <c>Reminder = 3</c> 
/// <c>SMS = 4</c> 
/// <c>Alarm = 5</c>
/// <c>Alarm2 = 6</c>
/// <c>Alarm3 = 7</c>
/// <c>Alarm4 = 8</c>
/// <c>Alarm5 = 9</c>
/// <c>Alarm6 = 10</c>
/// <c>Alarm7 = 11</c>
/// <c>Alarm8 = 12</c>
/// <c>Alarm9 = 13</c>
/// <c>Alarm10 = 14</c>
/// <c>Call = 15</c>
/// <c>Call1 = 16</c>
/// <c>Call2 = 17</c>
/// <c>Call3 = 18</c>
/// <c>Call4 = 19</c>
/// <c>Call5 = 20</c>
/// <c>Call6 = 21</c>
/// <c>Call7 = 22</c>
/// <c>Call8 = 23</c>
/// <c>Call9 = 24</c>
/// <c>Call10 = 25</c></param>
/// <param name="audioOption">Indicates the behavior of the audio on Toast visualization, <c>Default: 0</c>.
/// Can be:
/// <c>Default = 0</c>
/// <c>Silent = 1</c>
/// <c>Loop = 2</c></param>
DLLEXPORT void CreateToast(const wchar_t* appName,
	const wchar_t* appUserModelID,
	const wchar_t* toastMessage,
	ToastEventCallback eventCallback,
	const wchar_t* attribution = nullptr,
	int duration = 0,
	int audioFile = 0,
	int audioOption = 0);

//// text02 or 3
//DLLEXPORT void CreateToast(const wchar_t* appName,
//	const wchar_t* appUserModelID,
//	const wchar_t* toastTitle,
//	const wchar_t* toastMessage,
//	ToastEventCallback eventCallback,
//	const wchar_t* attribution = nullptr,
//	WinToastTemplate::Duration duration = WinToastTemplate::Duration::System,
//	WinToastTemplate::AudioSystemFile audioFile = WinToastTemplate::AudioSystemFile::DefaultSound, 
//	WinToastTemplate::AudioOption audioOption = WinToastTemplate::AudioOption::Default,
//	bool wrapFirst = true);
//
//// text04
//DLLEXPORT void CreateToast(const wchar_t* appName,
//	const wchar_t* appUserModelID,
//	const wchar_t* toastTitle,
//	const wchar_t* toastMessage,
//	const wchar_t* toastAdditionalMessage,
//	ToastEventCallback eventCallback,
//	const wchar_t* attribution = nullptr,
//	WinToastTemplate::Duration duration = WinToastTemplate::Duration::System,
//	WinToastTemplate::AudioSystemFile audioFile = WinToastTemplate::AudioSystemFile::DefaultSound, 
//	WinToastTemplate::AudioOption audioOption = WinToastTemplate::AudioOption::Default);
//
//// imageandtext01
//DLLEXPORT void CreateToast(const wchar_t* appName,
//	const wchar_t* appUserModelID,
//	const wchar_t* toastMessage,
//	const wchar_t* toastImagePath,
//	ToastEventCallback eventCallback,
//	const wchar_t* attribution = nullptr,
//	WinToastTemplate::Duration duration = WinToastTemplate::Duration::System,
//	WinToastTemplate::AudioSystemFile audioFile = WinToastTemplate::AudioSystemFile::DefaultSound, 
//	WinToastTemplate::AudioOption audioOption = WinToastTemplate::AudioOption::Default);
//
//// imageandtext02 or 3
//DLLEXPORT void CreateToast(const wchar_t* appName,
//	const wchar_t* appUserModelID,
//	const wchar_t* toastTitle,
//	const wchar_t* toastMessage,
//	const wchar_t* toastImagePath,
//	ToastEventCallback eventCallback,
//	const wchar_t* attribution = nullptr,
//	WinToastTemplate::Duration duration = WinToastTemplate::Duration::System,
//	WinToastTemplate::AudioSystemFile audioFile = WinToastTemplate::AudioSystemFile::DefaultSound, 
//	WinToastTemplate::AudioOption audioOption = WinToastTemplate::AudioOption::Default,
//	bool wrapFirst = true);
//
//// imageandtext04
//DLLEXPORT void CreateToast(const wchar_t* appName,
//	const wchar_t* appUserModelID,
//	const wchar_t* toastTitle,
//	const wchar_t* toastMessage,
//	const wchar_t* toastAdditionalMessage,
//	const wchar_t* toastImagePath,
//	ToastEventCallback eventCallback,
//	const wchar_t* attribution = nullptr,
//	WinToastTemplate::Duration duration = WinToastTemplate::Duration::System,
//	WinToastTemplate::AudioSystemFile audioFile = WinToastTemplate::AudioSystemFile::DefaultSound, 
//	WinToastTemplate::AudioOption audioOption = WinToastTemplate::AudioOption::Default);

// internal call, to generate every toast
void CreateToast(const wchar_t* appName,
	const wchar_t* appUserModelID,
	WinToastTemplate::WinToastTemplateType toastTemplate,
	ToastEventCallback eventCallback,
	const wchar_t* toastTitle = nullptr,
	const wchar_t* toastMessage = nullptr,
	const wchar_t* toastAdditionlMessage = nullptr,
	const wchar_t* toastImagePath = nullptr,
	const wchar_t* attribution = nullptr,
	WinToastTemplate::Duration duration = WinToastTemplate::Duration::System,
	WinToastTemplate::AudioSystemFile audioFile = WinToastTemplate::AudioSystemFile::DefaultSound,
	WinToastTemplate::AudioOption audioOption = WinToastTemplate::AudioOption::Default,
	INT64 expiration = -1,
	bool createShortcut = true);