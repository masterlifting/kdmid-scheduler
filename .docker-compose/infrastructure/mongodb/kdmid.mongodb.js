/** @format */

use('kdmid');

var kdmidValues = db.KdmidBotCommands.find({
  $where: function () {
    var kdmidIdStr = this.Command.Parameters['KdmidScheduler.Abstractions.Models.Core.v1.KdmidId'];
    if (kdmidIdStr) {
      var kdmidId = JSON.parse(kdmidIdStr);
      return kdmidId.id === '72095';
    }
    return false;
  },
});

kdmidValues.forEach(function (doc) {
  var parameters = doc.Command.Parameters;
  var kdmidId = JSON.parse(parameters['KdmidScheduler.Abstractions.Models.Core.v1.KdmidId']);

  kdmidId.cd = '68672fa8';

  parameters['KdmidScheduler.Abstractions.Models.Core.v1.KdmidId'] = JSON.stringify(kdmidId);

  var updateResult = db.KdmidBotCommands.updateOne({ _id: doc._id }, { $set: { 'Command.Parameters': parameters } });

  if (updateResult.matchedCount === 0) {
    print('No document matched');
  } else if (updateResult.modifiedCount === 0) {
    print('Document matched but not modified');
  } else {
    print('Document updated');
  }
});
