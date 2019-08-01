#include "stdafx.h"
#include "wintoastimpl.h"
#include <string>

using namespace WinToastLib;

class WinToastHandler : public IWinToastHandler {
public:

	explicit WinToastHandler(ToastEventCallback eventCallback)
	{
		m_eventCallback = eventCallback;
	}

	void toastActivated() const override
	{
		std::wcout << L"The user clicked in this toast" << std::endl;
		m_eventCallback(0);
	}

	void toastActivated(int actionIndex) const override
	{
		std::wcout << L"The user clicked on action #" << actionIndex << std::endl;
		m_eventCallback(16 + actionIndex);
	}

	void toastDismissed(WinToastDismissalReason state) const override
	{
		switch (state) {
		case UserCanceled:
			std::wcout << L"The user dismissed this toast" << std::endl;
			m_eventCallback(1);
			break;
		case TimedOut:
			std::wcout << L"The toast has timed out" << std::endl;
			m_eventCallback(2);
			break;
		case ApplicationHidden:
			std::wcout << L"The application hid the toast using ToastNotifier.hide()" << std::endl;
			m_eventCallback(3);
			break;
		default:
			std::wcout << L"Toast not activated" << std::endl;
			m_eventCallback(4);
			break;
		}
	}

	void toastFailed() const override
	{
		std::wcout << L"Error showing current toast" << std::endl;
		m_eventCallback(5);
	}
private:
	ToastEventCallback m_eventCallback;
};

// -------------------------- Exported methods 
DLLEXPORT void CreateToast_Text01(const wchar_t* appName,
	const wchar_t* appUserModelID,
	const wchar_t* toastMessage,
	ToastEventCallback eventCallback,
	const wchar_t* attribution,
	int duration,
	int audioFile, 
	int audioOption)
{
	CreateToast(
		appName, 
		appUserModelID, 
		WinToastTemplate::WinToastTemplateType::Text01,
		eventCallback,
		toastMessage,
		nullptr,
		nullptr,
		nullptr,
		attribution,
		static_cast<WinToastTemplate::Duration>(duration),
		static_cast<WinToastTemplate::AudioSystemFile>(audioFile),
		static_cast<WinToastTemplate::AudioOption>(audioOption));
}

DLLEXPORT void CreateToast_Text02(const wchar_t* appName,
	const wchar_t* appUserModelID,
	const wchar_t* toastTitle,
	const wchar_t* toastMessage,
	ToastEventCallback eventCallback,
	const wchar_t* attribution,
	bool wrapFirstLine,
	int duration,
	int audioFile, 
	int audioOption)
{
	CreateToast(
		appName, 
		appUserModelID, 
		wrapFirstLine ? WinToastTemplate::WinToastTemplateType::Text02 : WinToastTemplate::WinToastTemplateType::Text03,
		eventCallback,
		toastTitle,
		toastMessage,
		nullptr,
		nullptr,
		attribution,
		static_cast<WinToastTemplate::Duration>(duration),
		static_cast<WinToastTemplate::AudioSystemFile>(audioFile),
		static_cast<WinToastTemplate::AudioOption>(audioOption));
}

DLLEXPORT void CreateToast_Text03(const wchar_t* appName,
	const wchar_t* appUserModelID,
	const wchar_t* toastTitle,
	const wchar_t* toastMessage,
	const wchar_t* toastAdditionalMessage,
	ToastEventCallback eventCallback,
	const wchar_t* attribution,
	int duration,
	int audioFile, 
	int audioOption)
{
	CreateToast(
		appName, 
		appUserModelID, 
		WinToastTemplate::WinToastTemplateType::Text04,
		eventCallback,
		toastTitle,
		toastMessage,
		toastAdditionalMessage,
		nullptr,
		attribution,
		static_cast<WinToastTemplate::Duration>(duration),
		static_cast<WinToastTemplate::AudioSystemFile>(audioFile),
		static_cast<WinToastTemplate::AudioOption>(audioOption));
}

DLLEXPORT void CreateToast_ImageAndText01(const wchar_t* appName,
	const wchar_t* appUserModelID,
	const wchar_t* toastMessage,
	const wchar_t* toastImagePath,
	ToastEventCallback eventCallback,
	const wchar_t* attribution,
	int duration,
	int audioFile, 
	int audioOption)
{
	CreateToast(
		appName, 
		appUserModelID, 
		WinToastTemplate::WinToastTemplateType::ImageAndText01,
		eventCallback,
		toastMessage,
		nullptr,
		nullptr,
		toastImagePath,
		attribution,
		static_cast<WinToastTemplate::Duration>(duration),
		static_cast<WinToastTemplate::AudioSystemFile>(audioFile),
		static_cast<WinToastTemplate::AudioOption>(audioOption));
}

DLLEXPORT void CreateToast_ImageAndText02(const wchar_t* appName,
	const wchar_t* appUserModelID,
	const wchar_t* toastTitle,
	const wchar_t* toastMessage,
	const wchar_t* toastImagePath,
	ToastEventCallback eventCallback,
	const wchar_t* attribution,
	bool wrapFirstLine,
	int duration,
	int audioFile, 
	int audioOption)
{
	CreateToast(
		appName, 
		appUserModelID, 
		wrapFirstLine ? WinToastTemplate::WinToastTemplateType::ImageAndText02 : WinToastTemplate::WinToastTemplateType::ImageAndText03,
		eventCallback,
		toastTitle,
		toastMessage,
		nullptr,
		toastImagePath,
		attribution,
		static_cast<WinToastTemplate::Duration>(duration),
		static_cast<WinToastTemplate::AudioSystemFile>(audioFile),
		static_cast<WinToastTemplate::AudioOption>(audioOption));
}

DLLEXPORT void CreateToast_ImageAndText03(const wchar_t* appName,
	const wchar_t* appUserModelID,
	const wchar_t* toastTitle,
	const wchar_t* toastMessage,
	const wchar_t* toastAdditionalMessage,
	const wchar_t* toastImagePath,
	ToastEventCallback eventCallback,
	const wchar_t* attribution,
	int duration,
	int audioFile, 
	int audioOption)
{
	CreateToast(
		appName, 
		appUserModelID, 
		WinToastTemplate::WinToastTemplateType::ImageAndText04,
		eventCallback,
		toastTitle,
		toastMessage,
		toastAdditionalMessage,
		toastImagePath,
		attribution,
		static_cast<WinToastTemplate::Duration>(duration),
		static_cast<WinToastTemplate::AudioSystemFile>(audioFile),
		static_cast<WinToastTemplate::AudioOption>(audioOption));
}

void CreateToast(const wchar_t* appName,
	const wchar_t* appUserModelID,
	WinToastTemplate::WinToastTemplateType toastTemplate,
	ToastEventCallback eventCallback,
	const wchar_t* toastTitle,
	const wchar_t* toastMessage,
	const wchar_t* toastAdditionlMessage,
	const wchar_t* toastImagePath,
	const wchar_t* attribution,
	WinToastTemplate::Duration duration,
	WinToastTemplate::AudioSystemFile audioFile,
	WinToastTemplate::AudioOption audioOption,
	INT64 expiration,
	bool createShortcut) {

	if (!WinToast::isCompatible()) {
		std::wcerr << L"Error, your system in not supported!" << std::endl;
		eventCallback(SystemNotSupported);
	}

	WinToast::instance()->setAppName(appName);
	WinToast::instance()->setAppUserModelId(appUserModelID);

	if (createShortcut) {
		const auto shortcutResult = WinToast::instance()->createShortcut();
		if (shortcutResult < 0)
			eventCallback(InitializationFailure);
	}

	if (!WinToast::instance()->initialize()) {
		std::wcerr << L"Error, your system in not compatible!" << std::endl;
		eventCallback(InitializationFailure);
	}

	WinToastTemplate templ(toastTemplate);

	if (toastTitle)
		templ.setFirstLine(toastTitle);
	if (toastMessage)
		templ.setSecondLine(toastMessage);
	if (toastAdditionlMessage)
		templ.setThirdLine(toastAdditionlMessage);
	if (duration)
		templ.setDuration(duration);
	if (audioFile)
		templ.setAudioPath(audioFile);
	if (audioOption)
		templ.setAudioOption(audioOption);
	if (attribution)
		templ.setAttributionText(attribution);
	if (toastImagePath)
		templ.setImagePath(toastImagePath);

	if (expiration > 0)
		templ.setExpiration(expiration);

	if (WinToast::instance()->showToast(templ, new WinToastHandler(eventCallback)) < 0) {
		std::wcerr << L"Could not launch your toast notification!";
		eventCallback(ToastFailed);
	}

	eventCallback(AwaitingInteractions);
}