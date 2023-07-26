// Startwith regex
db.employee.aggregate([
    {
      $search: {
        index: "employeeSearch",
        compound: {
          must: [
            {
              equals: {
                path: "clientId",
                value: ObjectId(
                  "64baf1861b63c94b71d0c8fe"
                ),
              },
            },
          ],
          should: [
            {
              regex: {
                path: "firstName",
                query: "aa(.*)",
                allowAnalyzedField: true,
              },
            },
            {
              regex: {
                path: "lastName",
                query: "aa(.*)",
                allowAnalyzedField: true,
              },
            },
            {
              regex: {
                path: "email",
                query: "aa(.*)",
                allowAnalyzedField: true,
              },
            },
          ],
          minimumShouldMatch: 1,
        },
      },
    },
    { $sort: { lastName: 1, firstName: 1 } },
    {
      $project: {
        firstName: 1,
        lastName: 1,
        email: 1,
      },
    },
    {
      $facet: {
        rows: [{ $skip: 0 }, { $limit: 5 }],
        totalRows: [
          { $replaceWith: "$$SEARCH_META" },
          { $limit: 1 },
        ],
      },
    },
    {
      $set: {
        totalRows: {
          $arrayElemAt: [
            "$totalRows.count.lowerBound",
            0,
          ],
        },
      },
    },
  ]);