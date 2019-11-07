# Duty Finder Assist Plugin for ACT
### Based on the work from [devunt](https://github.com/devunt/DFAssist) and [lalafellsleep](https://github.com/lalafellsleep/ACTFate)

![releases](https://img.shields.io/github/tag/easly1989/ffxiv_act_dfassist.svg)
![issues](https://img.shields.io/github/issues/easly1989/ffxiv_act_dfassist.svg)
![license](https://img.shields.io/github/license/easly1989/ffxiv_act_dfassist.svg)
![downloads](https://img.shields.io/github/downloads/easly1989/ffxiv_act_dfassist/total.svg)
[![paypal](https://img.shields.io/badge/support%20me-on%20paypal-blue)](https://www.paypal.me/ruggierocarlo)

![main](https://github.com/easly1989/ffxiv_act_dfassist/blob/master/images/main.png)

## About this Plugin
A simple plugin to scout the next dungoeon/trial/raid (instance to be generic) you are going to enter.<br>
*It works only on windows 10 and, for the moment, only with final fantasy in Borderless Windowed Mode.*<br>
If you find any bug or have any idea to share and optimize the plugin fill free to contact me or open an Issue (:

## Telegram and Pushbullet Support!
As of version 2.0.0, you can now set up Telegram and Pushbullet notifications directly from within DFAssist plugin settings!

## TTS Support!
As of version 1.4.1, Text to Speech has been added, and can be enabled from the plugin page inside ACT!

## Automatic ACT Updates!
Starting from version 1.3.11 DFAssist supports AutoUpdating directly from within ACT
    - big thanks to [EQAditu](https://forums.advancedcombattracker.com/profile/EQAditu)

### Why another rework?!?
It seems like **lalafellsleep** simply stopped the development, and using an external tool (like the one from **devunt**) seems a bit like an overkill...<br>
... We already use ACT for various things, so integrate this feature seems like a better option (:

## Installation
### Download Binaries
You can download the latest binaries from [here](https://github.com/easly1989/ffxiv_act_dfassist/releases/latest)<br>

### Add the plugin to ACT
  - As usual add the plugin in ACT, after the [FFXIV_Parsing_Plugin](https://github.com/ravahn/FFXIV_ACT_Plugin) by [ravahn](https://github.com/ravahn)
    ![install](https://github.com/easly1989/ffxiv_act_dfassist/blob/master/images/install_1.png)
  - Once installed you can go into the plugin tab and edit the settings
  - You can check the default settings, or test your custom settings clicking the "Test Configuration" button located in the General Settings panel
	![settings](https://github.com/easly1989/ffxiv_act_dfassist/blob/master/images/install_2.png)
  - By default you should see this when clicking the Test Configuration button (unless you have changed the settings)
    ![toast](https://github.com/easly1989/ffxiv_act_dfassist/blob/master/images/install_3.png)
  - Congratulations! You did it! (:

## Building from Sources
Building should be really straight forward
#### Required components
 - [Visual Studio 2017 (any version will do)](https://visualstudio.microsoft.com/it/downloads/)
 - [Advanced Combat Tracker (latest version)](https://advancedcombattracker.com/includes/page-download.php?id=57)
#### How To Build
 1. Download or checkout the sources
 2. Open the **DFAssist.sln**
 3. Add the reference to your **Advanced Combat Tracker.exe** in **DFAssist.csproj**
 4. Build!
 
#### Side notes
It should not be a problem building for x86, but (as of the latest news from SquareEnix) you should build for x64, as the game dropped support for x86 with Shadowbringers!
 
### To-dos
- [x] Automatic Updates from within ACT
- [x] Remove unused support for Telegram
- [x] Remove unused support for FATEs
- [x] Handle Assembly resolve without locking dlls
- [x] Handle test mode, enable users to see instances code
- [x] add TTS support
- [x] add Telegram support
- [x] add PushBullet support
- [ ] add Discord support
- [ ] Inject toasts in game (to make it work even in full screen mode)

---

### If you like my work
<a href="https://www.paypal.me/ruggierocarlo">
  <img src="https://user-images.githubusercontent.com/3910202/35670996-5fb27278-073a-11e8-9a0a-7f951bbf04ff.png" width="25%" alt="Support with PayPal" />
</a>
