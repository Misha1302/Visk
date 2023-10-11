$str = (gci -include *.cs -recurse | select-string .).Count
echo "This project has $str not empty lines of code"
pause