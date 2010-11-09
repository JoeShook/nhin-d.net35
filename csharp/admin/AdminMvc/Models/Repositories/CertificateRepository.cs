using System.Linq;

using Health.Direct.Config.Client.CertificateService;
using Health.Direct.Config.Store;

namespace AdminMvc.Models.Repositories
{
    public interface ICertificateRepository : IRepository<Certificate>
    {
    }

    public class CertificateRepository : ICertificateRepository
    {
        private readonly CertificateStoreClient m_client;

        public CertificateRepository()
        {
            m_client = new CertificateStoreClient();
        }

        protected CertificateStoreClient Client { get { return m_client; } }
        
        public IQueryable<Certificate> FindAll()
        {
            return Client.EnumerateCertificates(0, int.MaxValue, null).AsQueryable();
        }

        //public Address Add(AddressModel model)
        //{
        //    return Client.AddAddress(
        //        new Address
        //            {
        //                DisplayName = model.DisplayName,
        //                DomainID = model.DomainID,
        //                EmailAddress = model.EmailAddress,
        //                Type = model.Type
        //            });
        //}

        public Certificate Add(Certificate certificate)
        {
            return Client.AddCertificate(certificate);
        }

        public void Delete(Certificate certificate)
        {
            Client.RemoveCertificates(new[] {certificate.ID});
        }

        public void Update(Certificate certificate)
        {
            Delete(certificate);
            Add(certificate);
        }

        public Certificate Get(long id)
        {
            // TODO: Replace with GetCertificateById()
            return (from certificate in FindAll()
                    where certificate.ID == id
                    select certificate).SingleOrDefault();
        }
    }
}