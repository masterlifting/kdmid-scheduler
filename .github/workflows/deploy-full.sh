#!/bin/bash

set -e

cd /usr/src/kdmid-scheduler

git stash
git pull origin main

chmod 600 .docker-compose/infrastructure/mongodb/mongodb-keyfile
chmod 600 .docker-compose/infrastructure/traefik/traefik.yml
chmod 600 .docker-compose/infrastructure/traefik/letsencrypt/acme.json
chown root:root .docker-compose/infrastructure/traefik/letsencrypt/acme.json

docker-compose -p kdmid -f .docker-compose/docker-compose.vps.yml up -d --build --force-recreate --remove-orphans