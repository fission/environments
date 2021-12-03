#!/bin/bash

num1=0
num2=1

for ((i=0;i<10;i++))
do
    echo -n "$num1"
    sum=$((num1+num2))
    num1=$num2
    num2=$sum
done
echo "Modules are awesome!"