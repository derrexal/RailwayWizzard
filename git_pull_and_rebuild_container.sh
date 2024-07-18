#!/bin/bash

# git_pull_and_rebuild_container.sh


echo "---> Pulling changes for remote repository"
status_pull=$(sudo git pull)
echo $status_pull
if [[ $status_pull == "*done*" ]];then
  echo "---> Pulling success"
  echo "---> Clearing disk space from Docker Garbage"
  sudo docker system prune -f -a

  echo "---> Containers rebuild and restart"
  sudo docker compose build
  sudo docker compose down
  sudo docker compose up -d
elif [[ $status_pull == "Already up to date."  ]];then
  echo "---> Project is actual"
else
  echo "---> Pulling failed"
fi
