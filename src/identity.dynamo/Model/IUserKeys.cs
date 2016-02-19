// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCamino.AspNet.Identity.Dynamo.Model
{
    public interface IUserKeys<TKey>
    {
        TKey Id { get; set; }

        TKey UserId { get; set; }

        string UserName { get; set; }

        string Email { get; set; }

    }
}
