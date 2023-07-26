// Startwith
db.employee.aggregate([
    {
      $search: {
        index: "employeeSearchAutocomplete",
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
              autocomplete: {
                path: "firstName",
                query: "gil",
              },
            },
            {
              autocomplete: {
                path: "lastName",
                query: "gil",
              },
            },
            {
              autocomplete: {
                path: "email",
                query: "gil",
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

  // Startwith fuzzy
  db.employee.aggregate([
    {
      $search: {
        index: "employeeSearchAutocomplete",
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
              autocomplete: {
                path: "firstName",
                query: "balos",
                fuzzy: {
                        "maxEdits": 1,
                        "prefixLength": 1,
                        "maxExpansions": 256
                      }
              },
            },
            {
              autocomplete: {
                path: "lastName",
                query: "balos",
                fuzzy: {
                        "maxEdits": 1,
                        "prefixLength": 1,
                        "maxExpansions": 256
                      }
              },
            },
            {
              autocomplete: {
                path: "email",
                query: "balos",
                fuzzy: {
                        "maxEdits": 1,
                        "prefixLength": 1,
                        "maxExpansions": 256
                      }
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
        rows: [{ $skip: 5 }, { $limit: 5 }],
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
    }
  ]);