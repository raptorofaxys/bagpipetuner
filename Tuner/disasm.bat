echo First parameter is build number in system temp folder
"C:\Program Files (x86)\Arduino\hardware\tools\avr/bin/avr-objdump" -D "C:\Users\Raptor\AppData\Local\Temp\arduino_build_%1%/Tuner.ino.elf" > Tuner.elf.s