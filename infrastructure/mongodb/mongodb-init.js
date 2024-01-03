rs.initiate(
  {
    _id: "rs0",
    version: 1,
    members: [
      {
        _id: 1,
        host: "mongo1:27017",
        priority: 2,
      },
      {
        _id: 2,
        host: "mongo2:27017",
        priority: 1,
      },
      {
        _id: 3,
        host: "mongo3:27017",
        priority: 1,
      },
    ],
  },
  { force: true }
);
// use admin
db.createUser({
  user: "kdmid",
  pwd: "kdmid",
  roles: [{ role: "root", db: "admin" }],
});
