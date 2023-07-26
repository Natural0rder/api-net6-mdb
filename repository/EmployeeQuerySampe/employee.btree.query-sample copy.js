// Startwith regex
db.employee.aggregate([
    {
      $match: {
        clientId: ObjectId(
          "64baf1861b63c94b71d0c8fe"
        ),
        $or: [
          { firstName: /^gil/i },
          { lastName: /^gil/i },
          { email: /^gil/i },
        ],
      },
    },
    {
      $project: {
        firstName: 1,
        lastName: 1,
        email: 1,
      },
    },
    {
      $facet: {
        count: [{ $count: "count" }],
        data: [
          { $sort: { lastName: 1, firstName: 1 } },
          { $skip: 5 },
          { $limit: 5 },
        ],
      },
    },
  ]);