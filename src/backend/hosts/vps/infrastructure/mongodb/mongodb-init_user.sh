#!/bin/bash
echo "Waiting for user to be created..."
sleep 2

userName=process.env.MONGODBCONNECTION_USERNAME
userPassword=process.env.MONGODBCONNECTION_PASSWORD

mongosh admin --eval 'db.createUser({
  user: "$userName",
  pwd: "$userPassword",
  roles: [{ role: "root", db: "admin" }]
});'