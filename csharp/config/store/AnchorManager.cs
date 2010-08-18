﻿/* 
 Copyright (c) 2010, NHIN Direct Project
 All rights reserved.

 Authors:
    Umesh Madan     umeshma@microsoft.com
  
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
Neither the name of the The NHIN Direct Project (nhindirect.org). nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using NHINDirect.Certificates;

namespace NHINDirect.Config.Store
{
    public class AnchorManager
    {
        ConfigStore m_store;
        
        internal AnchorManager(ConfigStore store)
        {
            m_store = store;
        }

        internal ConfigStore Store
        {
            get
            {
                return m_store;
            }
        }
        
        public void Add(Anchor anchor)
        {
            using (ConfigDatabase db = this.Store.CreateContext())
            {
                this.Add(db, anchor);
                db.SubmitChanges();
            }
        }

        public void Add(IEnumerable<Anchor> anchors)
        {
            if (anchors == null)
            {
                throw new ArgumentNullException();
            }
            using (ConfigDatabase db = this.Store.CreateContext())
            {
                foreach(Anchor anchor in anchors)
                {
                    this.Add(db, anchor);
                }
                db.SubmitChanges();
            }
        }
        
        public void Add(ConfigDatabase db, Anchor anchor)
        {
            if (db == null)
            {
                throw new ArgumentException();
            }
            if (anchor == null)
            {
                throw new ConfigStoreException(ConfigStoreError.InvalidAnchor);
            }
            
            db.Anchors.InsertOnSubmit(anchor);
        }

        public Anchor[] Get(long[] certIDs)
        {
            if (certIDs == null || certIDs.Length == 0)
            {
                throw new ConfigStoreException(ConfigStoreError.InvalidIDs);
            }

            using (ConfigDatabase db = this.Store.CreateReadContext())
            {
                return db.Anchors.Get(certIDs).ToArray();
            }
        }

        public Anchor[] Get(long lastCertID, int maxResults)
        {
            using (ConfigDatabase db = this.Store.CreateReadContext())
            {
                return this.Get(db, lastCertID, maxResults).ToArray();
            }
        }

        public IEnumerable<Anchor> Get(ConfigDatabase db, long lastCertID, int maxResults)
        {
            if (db == null)
            {
                throw new ArgumentNullException();
            }

            return db.Anchors.Get(lastCertID, maxResults);
        }

        public Anchor[] Get(string owner)
        {
            using (ConfigDatabase db = this.Store.CreateContext())
            {
                return this.Get(db, owner).ToArray();
            }
        }

        public IEnumerable<Anchor> Get(ConfigDatabase db, string owner)
        {
            if (db == null)
            {
                throw new ArgumentNullException();
            }
            if (string.IsNullOrEmpty(owner))
            {
                throw new ConfigStoreException(ConfigStoreError.InvalidOwnerName);
            }
            
            return db.Anchors.Get(owner);
        }

        public Anchor[] GetIncoming(string ownerName)
        {
            if (string.IsNullOrEmpty(ownerName))
            {
                throw new ConfigStoreException(ConfigStoreError.InvalidOwnerName);
            }

            using (ConfigDatabase db = this.Store.CreateContext())
            {
                return db.Anchors.GetIncoming(ownerName).ToArray();
            }
        }

        public Anchor[] GetOutgoing(string ownerName)
        {
            if (string.IsNullOrEmpty(ownerName))
            {
                throw new ConfigStoreException(ConfigStoreError.InvalidOwnerName);
            }

            using (ConfigDatabase db = this.Store.CreateReadContext())
            {
                return db.Anchors.GetOutgoing(ownerName).ToArray();
            }
        }

        public Anchor Get(string owner, string thumbprint)
        {
            using (ConfigDatabase db = this.Store.CreateReadContext())
            {
                return this.Get(db, owner, thumbprint);
            }
        }

        public Anchor Get(ConfigDatabase db, string owner, string thumbprint)
        {
            if (db == null)
            {
                throw new ArgumentNullException();
            }
            if (string.IsNullOrEmpty(owner))            
            {
                throw new ConfigStoreException(ConfigStoreError.InvalidOwnerName);
            }
            if (string.IsNullOrEmpty(thumbprint))
            {
                throw new ConfigStoreException(ConfigStoreError.InvalidThumbprint);
            }
            return db.Anchors.Get(owner, thumbprint);
        }

        public void Remove(long[] certificateIDs)
        {
            using (ConfigDatabase db = this.Store.CreateContext())
            {
                this.Remove(db, certificateIDs);

                // We don't commit, because we execute deletes directly
            }
        }

        public void Remove(ConfigDatabase db, long[] certificateIDs)
        {
            if (db == null)
            {
                throw new ArgumentNullException();
            }
            if (certificateIDs == null || certificateIDs.Length == 0)
            {
                throw new ConfigStoreException(ConfigStoreError.InvalidIDs);
            }
            //
            // Todo: this in a single query
            //
            for (int i = 0; i < certificateIDs.Length; ++i)
            {
                db.Anchors.ExecDelete(certificateIDs[i]);
            }
        }

        public void Remove(string owner, string thumbprint)
        {
            using (ConfigDatabase db = this.Store.CreateContext())
            {
                this.Remove(db, owner, thumbprint);
            }
        }

        public void Remove(ConfigDatabase db, string owner, string thumbprint)
        {
            if (db == null)
            {
                throw new ArgumentNullException();
            }
            if (string.IsNullOrEmpty(owner))
            {
                throw new ConfigStoreException(ConfigStoreError.InvalidOwnerName);
            }
            if (string.IsNullOrEmpty(thumbprint))
            {
                throw new ConfigStoreException(ConfigStoreError.InvalidThumbprint);
            }
            
            db.Anchors.ExecDelete(owner, thumbprint);
        }

        public void Remove(string ownerName)
        {
            if (string.IsNullOrEmpty(ownerName))
            {
                throw new ConfigStoreException(ConfigStoreError.InvalidOwnerName);
            }

            using (ConfigDatabase db = this.Store.CreateContext())
            {
                this.Remove(db, ownerName);
            }
        }

        public void Remove(ConfigDatabase db, string ownerName)
        {
            if (db == null)
            {
                throw new ArgumentNullException();
            }
            if (string.IsNullOrEmpty(ownerName))
            {
                throw new ConfigStoreException(ConfigStoreError.InvalidOwnerName);
            }

            db.Anchors.ExecDelete(ownerName);
        }
    }
 }