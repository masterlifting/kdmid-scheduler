/** @format */

use('kdmid');

var kdmidValues = db.KdmidBotCommands.find({
  $where: function () {
    var cityStr = this.Command.Parameters['v1.City'];
    if (cityStr) {
      var city = JSON.parse(cityStr);
      return city.Code === 'Ljubljana';
    }
    return false;
  },
});

// kdmidValues.forEach(function (doc) {
//   var parameters = doc.Command.Parameters;
//   var kdmidId = JSON.parse(parameters['v1.KdmidId']);

//   kdmidId.cd = '68672fa8';

//   parameters['v1.KdmidId'] = JSON.stringify(kdmidId);

//   var updateResult = db.KdmidBotCommands.updateOne({ _id: doc._id }, { $set: { 'Command.Parameters': parameters } });

//   if (updateResult.matchedCount === 0) {
//     print('No document matched');
//   } else if (updateResult.modifiedCount === 0) {
//     print('Document matched but not modified');
//   } else {
//     print('Document updated');
//   }
// });
