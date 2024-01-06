#!/bin/bash
echo "Waiting for user to be created..."
sleep 2

mongosh admin --eval 'db.createUser({
  user: "admin",
  pwd: "adminPassword",
  roles: [{ role: "root", db: "admin" }]
});'