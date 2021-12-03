#!/bin/sh

num1=0
num2=1

i=0
while [ "$i" -le 10 ]
do
    echo "$num1 "
    sum=$((num1+num2))
    num1=$num2
    num2=$sum
    i=$(( i + 1 ))
done
echo "Modules are awesome!"