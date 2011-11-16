﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sones.GraphDB.Expression;

namespace sones.GraphDS.GraphDSRemoteClient.sonesGraphDSRemoteAPI
{
    public partial class ServiceCollectionLiteralExpression
    {
        internal ServiceCollectionLiteralExpression(CollectionLiteralExpression myExpression)
        {
            this.CollectionLiteral = myExpression.CollectionLiteral.Select(x => (object)x).ToArray();
        }
    }
}
