#!/bin/bash

echo "Waiting for replica set to be created..."
sleep 2

mongosh --eval 'rs.initiate({
    _id: "rs0",
    version: 1,
    members: [
        { _id: 1, host: "mongodb_1:27017", priority: 2 },
        { _id: 2, host: "mongodb_2:27017", priority: 1 },
        { _id: 3, host: "mongodb_3:27017", priority: 1 },
    ]}, { force: true });'

tail -f /dev/null
