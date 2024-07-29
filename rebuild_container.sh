#!/bin/bash

# rebuild_container.sh
echo "---> Clearing disk space from Docker Garbage"
sudo docker system prune -f -a

echo "---> Containers rebuild and restart"
sudo docker compose build
sudo docker compose down
sudo docker compose up -d