Few changes from the official build :

"Color by Battery %" no longer work, instead if this case is check, it will color the lightbar according to the xinput slot (blue, red, green, purple)
You need a VigemBus that report correctly the xinput info, back in september 2023, i had to roll back to ViGEmBus 1.17.333.0 , idk if the problem was fix.

LoadProfile.0. : DS4Windows support commands like (for exemple) LoadProfile.1.Default that change the device 1 profile to Default. Now, if you send LoadProfile.0.Default it will change ALL the devices to Default.

Xbox Knockoff mode : In the profile, you can set the controller type as "Xbox Compatible" instead of "Xbox 360" and "DualShock 4".
Xbox compatible works exactly like Xbox 360 but use vendorId and productID of a logitech Gamepad F710 (useful if you want to differentiate your controllers from other 360 devices)

If, using LoadProfile command, you switch from a profile that doesn't contain the word "lag" to a profile that contain the word "lag", it will restart DS4Win. Reason behind this dirty hack is that i have some profile "nolag" with Enable OutPut Data unchecked, you get less input lag that way but to get the benefit, it's needed to restart ds4win.

