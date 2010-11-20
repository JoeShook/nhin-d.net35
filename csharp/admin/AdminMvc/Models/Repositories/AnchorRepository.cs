/* 
 Copyright (c) 2010, Direct Project
 All rights reserved.

 Authors:
    John Theisen
  
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
Neither the name of The Direct Project (directproject.org) nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 
*/
using System;
using System.Linq;

using Health.Direct.Config.Client.CertificateService;
using Health.Direct.Config.Store;

namespace Health.Direct.Admin.Console.Models.Repositories
{
    public class AnchorRepository : IAnchorRepository
    {
        private readonly IAnchorStore m_client;

        public AnchorRepository(IAnchorStore client)
        {
            m_client = client;
        }

        protected IAnchorStore Client { get { return m_client; } }
        
        public IQueryable<Anchor> Query()
        {
            return Client.EnumerateAnchors(0, int.MaxValue, null).AsQueryable();
        }

        public Anchor Add(Anchor anchor)
        {
            return Client.AddAnchor(anchor);
        }

        public void Delete(Anchor anchor)
        {
            Client.RemoveAnchors(new[] {anchor.ID});
        }

        public void Update(Anchor anchor)
        {
            throw new NotSupportedException("Updating anchors not supported");
        }

        public Anchor Get(long id)
        {
            return Client.GetAnchors(new[] {id}, null).SingleOrDefault();
        }

        public Anchor ChangeStatus(Anchor anchor, EntityStatus status)
        {
            Client.SetAnchorStatus(new[] { anchor.ID }, status);
            anchor.Status = status;
            return anchor;
        }
    }
}