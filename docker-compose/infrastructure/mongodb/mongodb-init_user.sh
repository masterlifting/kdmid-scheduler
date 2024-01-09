echo "Waiting for user to be created..."
sleep 2

userName=$MONGODBCONNECTION_USERNAME
userPassword=$MONGODBCONNECTION_PASSWORD

mongosh admin --eval 'db.createUser({
  user: "$userName",
  pwd: "$userPassword",
  roles: [{ role: "root", db: "admin" }]
});'

echo "User created successfully"