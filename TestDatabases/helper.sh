#!/bin/bash
#Removes the images that don't exist in the .accdb
#Assuming the consecutive set of record exists

image_directory="" #location of the ReferenceImages folder in the section
lower= # the last 3 number in the filename (e.g. 2019-06-28_13-37-27_464.jpg) of the FIRST image in database
upper= # the last 3 number in the filename (e.g. 2019-06-28_13-37-27_464.jpg) of the LAST image in database

for image in "$image_directory"/*.jpg
do
  number=$(echo "$image" | sed -r "s/.+\/(.+)\..+/\1/" | sed -r "s/.+_(.+)/\1/")
  echo "$image"
  if [ "$number" -lt "$lower" ] || [ "$number" -gt "$upper" ]
  then
    echo "$image"
    rm -rf "$image"
  fi
done