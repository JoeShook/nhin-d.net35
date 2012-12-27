package org.nhindirect.config.service;

import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.never;
import static org.mockito.Matchers.any;
import static org.mockito.Matchers.eq;

import java.security.cert.X509Certificate;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Collection;
import java.util.Locale;

import junit.framework.TestCase;

import org.apache.camel.ProducerTemplate;
import org.nhindirect.config.service.impl.TrustBundleServiceImpl;
import org.nhindirect.config.store.BundleRefreshError;
import org.nhindirect.config.store.Certificate;
import org.nhindirect.config.store.Certificate.CertContainer;
import org.nhindirect.config.store.TrustBundle;
import org.nhindirect.config.store.TrustBundleAnchor;
import org.nhindirect.config.store.dao.TrustBundleDao;

public class TrustBundleServiceTest extends TestCase
{
	private TrustBundleServiceImpl impl;
	private TrustBundleDao dao;
	private ProducerTemplate template;
	
	@Override
	public void setUp()
	{
		dao = mock(TrustBundleDao.class);
		template = mock(ProducerTemplate.class);
		
		impl = new TrustBundleServiceImpl();
		impl.setDao(dao);
		impl.setTemplate(template);
	}
	
	public void testGetTrustBundles() throws Exception
	{
		impl.getTrustBundles(false);
		
		verify(dao, times(1)).getTrustBundles();
		
		impl.getTrustBundles(true);
		
		verify(dao, times(2)).getTrustBundles();		
	}
	
	public void testGetTrustBundleByName() throws Exception
	{
		impl.getTrustBundleByName("test");
		
		verify(dao, times(1)).getTrustBundleByName("test");
	}
	
	
	public void testGetTrustBundleById() throws Exception
	{
		impl.getTrustBundleById(1234);
		
		verify(dao, times(1)).getTrustBundleById(1234);
	}		
	
	public void testAddTrustBundle() throws Exception
	{
		final TrustBundle bundle = new TrustBundle();
		impl.addTrustBundle(bundle);
		
		verify(dao, times(1)).addTrustBundle(bundle);
	}
	
	public void testRefresTrustBundle() throws Exception
	{
		final TrustBundle bundle = new TrustBundle();
		impl.refreshTrustBundle(bundle.getId());
		
		verify(dao, times(1)).getTrustBundleById(bundle.getId());
		verify(template, never()).sendBody(bundle);
		
		when(dao.getTrustBundleById(bundle.getId())).thenReturn(bundle);
		impl.refreshTrustBundle(bundle.getId());
		verify(dao, times(2)).getTrustBundleById(bundle.getId());
		verify(template, times(1)).sendBody(bundle);
	}
	
	public void testUpdateTrustBundleAnchors() throws Exception
	{
		final Calendar now = Calendar.getInstance(Locale.getDefault());
		final Collection<TrustBundleAnchor> anchors = new ArrayList<TrustBundleAnchor>();
		
		impl.updateTrustBundleAnchors(1234, now, anchors);
		
		verify(dao, times(1)).updateTrustBundleAnchors(1234, now, anchors);
	}	
	
	public void testUpdateLastUpdateError() throws Exception
	{
		final Calendar now = Calendar.getInstance(Locale.getDefault());
		
		impl.updateLastUpdateError(1234, now, BundleRefreshError.SUCCESS);
		
		verify(dao, times(1)).updateLastUpdateError(1234, now, BundleRefreshError.SUCCESS);
	}	
	
	public void testDeleteTrustBundles() throws Exception
	{	
		impl.deleteTrustBundles(new long[] {1,2,3});
		
		verify(dao, times(1)).deleteTrustBundles(new long[] {1,2,3});
	}	
	
	public void testUpdateTrustBundleSigningCertificate() throws Exception
	{	
		X509Certificate cert = mock(X509Certificate.class); 
		
		CertContainer container = mock(CertContainer.class);
		when(container.getCert()).thenReturn(cert);
		
		Certificate confCert = mock(Certificate.class);
		when(confCert.toCredential()).thenReturn(container);
		
		impl.updateTrustBundleSigningCertificate(1234, confCert);
		
		verify(dao, times(1)).updateTrustBundleSigningCertificate(eq((long)1234), (X509Certificate)any());
	}		
}