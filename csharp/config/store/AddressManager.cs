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
using System.Net.Mail;

namespace NHINDirect.Config.Store
{
    public class AddressManager : IEnumerable<Address>
    {
        ConfigStore m_store;
        
        internal AddressManager(ConfigStore store)
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
        
        public void Add(long domainID, string emailAddress)
        {
            this.Add(new Address(domainID, emailAddress));
        }
        
        public void Add(Address address)
        {
            using (ConfigDatabase db = this.Store.CreateContext())
            {
                this.Add(db, address);
                db.SubmitChanges();
            }
        }
        
        public void Add(IEnumerable<Address> addresses)
        {
            if (addresses == null)
            {
                throw new ArgumentNullException();
            }
            
            using (ConfigDatabase db = this.Store.CreateContext())
            {
                foreach(Address address in addresses)
                {
                    this.Add(db, address);
                }
                
                db.SubmitChanges();
            }            
        }
        
        public void Add(ConfigDatabase db, Address address)
        {
            if (db == null)
            {
                throw new ArgumentNullException();
            }
            
            if (address == null)
            {
                throw new ConfigStoreException(ConfigStoreError.InvalidAddress);
            }
            
            db.Addresses.InsertOnSubmit(address);
        }
        
        public void Update(Address address)
        {
            using (ConfigDatabase db = this.Store.CreateContext())
            {
                this.Update(db, address);
                db.SubmitChanges();
            }
        }

        public void Update(IEnumerable<Address> addresses)
        {
            if (addresses == null)
            {
                throw new ArgumentNullException();
            }
            using (ConfigDatabase db = this.Store.CreateContext())
            {
                foreach(Address address in addresses)
                {
                    this.Update(db, address);
                }
                db.SubmitChanges();
            }
        }
        
        public void Update(ConfigDatabase db, Address address)
        {
            if (db == null)
            {
                throw new ArgumentNullException();
            }
            if (address == null)
            {
                throw new ConfigStoreException(ConfigStoreError.InvalidAddress);
            }

            Address current = this.Get(db, address.EmailAddress);
            if (current == null)
            {
                this.Add(db, address);
            }
            else
            {
                address.UpdateDate = DateTime.Now;
                db.Addresses.Attach(address, current);
            }
        }
               
        public Address Get(string emailAddress)
        {
            using(ConfigDatabase db = this.Store.CreateReadContext())
            {
                return this.Get(db, emailAddress);
            }
        }
        
        public Address Get(ConfigDatabase db, string emailAddress)
        {
            if (db == null)
            {
                throw new ArgumentException();
            }
            
            return db.Addresses.Get(emailAddress);
        }

        public Address[] Get(string[] emailAddresses)
        {
            using (ConfigDatabase db = this.Store.CreateReadContext())
            {
                return this.Get(db, emailAddresses).ToArray();
            }
        }

        public IEnumerable<Address> Get(ConfigDatabase db, string[] emailAddresses)
        {
            if (db == null)
            {
                throw new ArgumentException();
            }
            
            this.VerifyEmailAddresses(emailAddresses);
            return db.Addresses.Get(emailAddresses);
        }

        public Address[] Get(long domainID, long lastAddressID, int maxResults)
        {
            using(ConfigDatabase db = this.Store.CreateReadContext())
            {
                return this.Get(db, domainID, lastAddressID, maxResults).ToArray();
            }
        }

        public IEnumerable<Address> Get(ConfigDatabase db, long domainID, long lastAddressID, int maxResults)
        {
            if (db == null)
            {
                throw new ArgumentNullException();
            }
            
            return db.Addresses.Get(domainID, lastAddressID, maxResults);
        }

        public Address[] Get(long lastAddressID, int maxResults)
        {
            using (ConfigDatabase db = this.Store.CreateReadContext())
            {
                return this.Get(db, lastAddressID, maxResults).ToArray();
            }
        }

        public IEnumerable<Address> Get(ConfigDatabase db, long lastAddressID, int maxResults)
        {
            if (db == null)
            {
                throw new ArgumentNullException();
            }

            return db.Addresses.Get(lastAddressID, maxResults);
        }
        
        public void Remove(string emailAddress)
        {
            using(ConfigDatabase db = this.Store.CreateContext())
            {
                this.Remove(db, emailAddress);
            }
        }
        
        public void Remove(ConfigDatabase db, string emailAddress)
        {
            if (string.IsNullOrEmpty(emailAddress))
            {
                throw new ConfigStoreException(ConfigStoreError.InvalidEmailAddress);
            }
            
            db.Addresses.ExecDelete(emailAddress);
        }

        public void Remove(IEnumerable<string> emailAddresses)
        {
            using (ConfigDatabase db = this.Store.CreateContext())
            {
                foreach(string emailAddress in emailAddresses)
                {
                    this.Remove(db, emailAddress);
                }
            }
        }
        
        public void RemoveDomain(long domainID)
        {
            using (ConfigDatabase db = this.Store.CreateContext())
            {
                this.RemoveDomain(db, domainID);
            }
        }

        public void RemoveDomain(ConfigDatabase db, long domainID)
        {
            db.Addresses.ExecDeleteDomain(domainID);
        }
        
        public void SetStatus(string[] emailAddresses, EntityStatus status)
        {
        }
        
        public void SetStatus(ConfigDatabase db, string[] emailAddresses, EntityStatus status)
        {
            if (db == null)
            {
                throw new ArgumentNullException();
            }
            
            this.VerifyEmailAddresses(emailAddresses);
            db.Addresses.ExecSetStatus(emailAddresses, status);
        }
        
        void VerifyEmailAddresses(string[] emailAddresses)
        {
            if (emailAddresses == null || emailAddresses.Length == 0)
            {
                throw new ConfigStoreException(ConfigStoreError.InvalidEmailAddress);
            }
            for (int i = 0; i < emailAddresses.Length; ++i)
            {
                if (string.IsNullOrEmpty(emailAddresses[i]))
                {
                    throw new ConfigStoreException(ConfigStoreError.InvalidEmailAddress);
                }
            }
        }

        public IEnumerator<Address> GetEnumerator()
        {
            using (ConfigDatabase db = this.Store.CreateContext())
            {
                foreach (Address address in db.Addresses)
                {
                    yield return address;
                }
            }
        }


        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}