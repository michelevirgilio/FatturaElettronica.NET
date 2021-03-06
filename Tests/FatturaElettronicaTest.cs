﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using System.IO;
using FatturaElettronica.Impostazioni;
using System;

namespace Tests
{
    [TestClass]
    public class FatturaElettronicaTest
    {
        [TestMethod]
        public void CreateInstancePubblicaAmministrazione()
        {
            var f = FatturaElettronica.FatturaElettronica.CreateInstance(Instance.PubblicaAmministrazione);
            Assert.AreEqual(FormatoTrasmissione.PubblicaAmministrazione, f.FatturaElettronicaHeader.DatiTrasmissione.FormatoTrasmissione);
        }
        [TestMethod]
        public void CreateInstancePrivati()
        {
            var f = FatturaElettronica.FatturaElettronica.CreateInstance(Instance.Privati);
            Assert.AreEqual(FormatoTrasmissione.Privati, f.FatturaElettronicaHeader.DatiTrasmissione.FormatoTrasmissione);
            Assert.AreEqual(new string('0', 7), f.FatturaElettronicaHeader.DatiTrasmissione.CodiceDestinatario);
        }

        [TestMethod]
        public void ValidateFormatoTrasmissione()
        {
            var f = FatturaElettronica.FatturaElettronica.CreateInstance(Instance.Privati);
            f.FatturaElettronicaHeader.DatiTrasmissione.FormatoTrasmissione = "test";
            Assert.IsTrue(f.Error.Contains("DatiTrasmissione.FormatoTrasmissione"));
            Assert.IsTrue(f.Error.Contains(FormatoTrasmissione.Privati));
            f.FatturaElettronicaHeader.DatiTrasmissione.FormatoTrasmissione =FormatoTrasmissione.Privati;
            Assert.IsFalse(f.Error.Contains("DatiTrasmissione.FormatoTrasmissione"));

            f = FatturaElettronica.FatturaElettronica.CreateInstance(Instance.PubblicaAmministrazione);
            f.FatturaElettronicaHeader.DatiTrasmissione.FormatoTrasmissione = "test";
            Assert.IsTrue(f.Error.Contains("DatiTrasmissione.FormatoTrasmissione"));
            Assert.IsTrue(f.Error.Contains(FormatoTrasmissione.PubblicaAmministrazione));
            f.FatturaElettronicaHeader.DatiTrasmissione.FormatoTrasmissione = FormatoTrasmissione.PubblicaAmministrazione;
            Assert.IsFalse(f.Error.Contains("DatiTrasmissione.FormatoTrasmissione"));
        }

        [TestMethod]
        public void ValidateCodiceDestinatario()
        {
            var f = FatturaElettronica.FatturaElettronica.CreateInstance(Instance.Privati);

            // CodiceDestinatario è obbligatorio
            Assert.IsTrue(f.Error.Contains("CodiceDestinatario"));

            // Nelle fatture tra privati CodiceDestinatario deve essere 7 caratteri.
            f.FatturaElettronicaHeader.DatiTrasmissione.CodiceDestinatario = "123456";
            Assert.IsTrue(f.Error.Contains("CodiceDestinatario"));
            f.FatturaElettronicaHeader.DatiTrasmissione.CodiceDestinatario = "12345678";
            Assert.IsTrue(f.Error.Contains("CodiceDestinatario"));
            f.FatturaElettronicaHeader.DatiTrasmissione.CodiceDestinatario = "1234567";
            Assert.IsFalse(f.Error.Contains("CodiceDestinatario"));

            f = FatturaElettronica.FatturaElettronica.CreateInstance(Instance.PubblicaAmministrazione);

            // CodiceDestinatario è obbligatorio
            Assert.IsTrue(f.Error.Contains("CodiceDestinatario"));

            // Nelle fatture PA CodiceDestinatario deve essere 6 caratteri.
            f.FatturaElettronicaHeader.DatiTrasmissione.CodiceDestinatario = "12345";
            Assert.IsTrue(f.Error.Contains("CodiceDestinatario"));
            f.FatturaElettronicaHeader.DatiTrasmissione.CodiceDestinatario = "1234567";
            Assert.IsTrue(f.Error.Contains("CodiceDestinatario"));
            f.FatturaElettronicaHeader.DatiTrasmissione.CodiceDestinatario = "123456";
            Assert.IsFalse(f.Error.Contains("CodiceDestinatario"));

        }


        [TestMethod]
        public void ValidatePECDestinatario()
        {
            var f = FatturaElettronica.FatturaElettronica.CreateInstance(Instance.Privati);

            // Nelle fatture tra privati, poiché il default per CodiceDestinatario è 0000000,
            // è necessario impostare un valore per PECDestinatario.
            Assert.IsTrue(f.Error.Contains("PECDestinatario"));

            // PECDestinatario deve essere tra 7 e 256 caratteri.
            f.FatturaElettronicaHeader.DatiTrasmissione.PECDestinatario = "123456";
            Assert.IsTrue(f.Error.Contains("PECDestinatario"));
            f.FatturaElettronicaHeader.DatiTrasmissione.PECDestinatario = new string('n', 257);
            Assert.IsTrue(f.Error.Contains("PECDestinatario"));
            f.FatturaElettronicaHeader.DatiTrasmissione.PECDestinatario = "1234567";
            Assert.IsFalse(f.Error.Contains("PECDestinatario"));

            // Se CodiceDestinatario è diverso da 0000000, allora PECDestinatario non deve essere
            // valorizzato.
            f.FatturaElettronicaHeader.DatiTrasmissione.CodiceDestinatario = "test";
            Assert.IsTrue(f.Error.Contains("PECDestinatario"));
            f.FatturaElettronicaHeader.DatiTrasmissione.PECDestinatario = null;
            Assert.IsFalse(f.Error.Contains("PECDestinatario"));
        }

        [TestMethod]
        public void StabileOrganizzazione()
        {
            var f = FatturaElettronica.FatturaElettronica.CreateInstance(Instance.PubblicaAmministrazione);

            // StabileOrganizzazione è disponibile sia in CedentePrestatore...
            f.FatturaElettronicaHeader.CedentePrestatore.StabileOrganizzazione.Indirizzo = "indirizzo1";
            // ...che in CommissionarioCommittente.
            f.FatturaElettronicaHeader.CessionarioCommittente.StabileOrganizzazione.Indirizzo = "indirizzo2";

        }

        [TestMethod]
        public void CessionarioCommittenteRappresentanteFiscale()
        {
            var f = FatturaElettronica.FatturaElettronica.CreateInstance(Instance.Privati);

            f.FatturaElettronicaHeader.CessionarioCommittente.RappresentanteFiscale.Denominazione = "denominazione1";
            f.FatturaElettronicaHeader.CessionarioCommittente.RappresentanteFiscale.Nome = "Nome";
            f.FatturaElettronicaHeader.CessionarioCommittente.RappresentanteFiscale.Cognome = "Cognome";
            Assert.IsTrue(f.Error.Contains("[Denominazione, CognomeNome]"));
            f.FatturaElettronicaHeader.CessionarioCommittente.RappresentanteFiscale.Denominazione = null;
            Assert.IsFalse(f.Error.Contains("[Denominazione, CognomeNome]"));

        }

        [TestMethod]
        public void ValidateModalitàPagamento()
        {
            FatturaElettronica.FatturaElettronicaBody.DatiPagamento.DettaglioPagamento dp;
            const int max = 22;

            for (var i=1; i<=max; i++)
            {
                dp = new FatturaElettronica.FatturaElettronicaBody.DatiPagamento.DettaglioPagamento();
                dp.ModalitaPagamento = string.Format("MP{0}", (i<10) ? "0"+i.ToString() : i.ToString());
                Assert.IsTrue(dp.IsValid);
            }

            dp = new FatturaElettronica.FatturaElettronicaBody.DatiPagamento.DettaglioPagamento();
            dp.ModalitaPagamento = string.Format("MP{0}", max+1);
            Assert.IsFalse(dp.IsValid);

            dp = new FatturaElettronica.FatturaElettronicaBody.DatiPagamento.DettaglioPagamento();
            dp.ModalitaPagamento = "test";
            Assert.IsFalse(dp.IsValid);

        }

        [TestMethod]
        public void ValidateNatura()
        {
            const int max = 7;

            // DatiGenerali.DatiCassaPrevidenziale.Natura.
            FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiCassaPrevidenziale dc;
            for (var i=1; i<=max; i++)
            {
                dc = new FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiCassaPrevidenziale();
                dc.Natura = string.Format("N{0}", i.ToString());
                Assert.IsFalse(dc.Error.Contains("Natura"));
            }
            dc = new FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiCassaPrevidenziale();
            dc.Natura = string.Format("N{0}", max+1);
            Assert.IsTrue(dc.Error.Contains("Natura"));
            dc = new FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiCassaPrevidenziale();
            dc.Natura = "test";
            Assert.IsTrue(dc.Error.Contains("Natura"));

            // DatiBeniServizi.DatiRiepilogo.Natura.
            FatturaElettronica.FatturaElettronicaBody.DatiBeniServizi.DatiRiepilogo dr;
            for (var i=1; i<=max; i++)
            {
                dr = new FatturaElettronica.FatturaElettronicaBody.DatiBeniServizi.DatiRiepilogo();
                dr.Natura = string.Format("N{0}", i.ToString());
                Assert.IsTrue(dr.IsValid);
            }
            dr = new FatturaElettronica.FatturaElettronicaBody.DatiBeniServizi.DatiRiepilogo();
            dr.Natura = string.Format("N{0}", max+1);
            Assert.IsFalse(dr.IsValid);
            dr = new FatturaElettronica.FatturaElettronicaBody.DatiBeniServizi.DatiRiepilogo();
            dr.Natura = "test";
            Assert.IsFalse(dr.IsValid);

        }

        [TestMethod]
        public void ValidateIBAN()
        {
            const int min = 15;
            const int max = 34;

            FatturaElettronica.FatturaElettronicaBody.DatiPagamento.DettaglioPagamento dp;
            for (var i=min; i<=max; i++)
            {
                dp = new FatturaElettronica.FatturaElettronicaBody.DatiPagamento.DettaglioPagamento();
                dp.IBAN = new string('n', i);
                Assert.IsFalse(dp.Error.Contains("IBAN"));
            }
            dp = new FatturaElettronica.FatturaElettronicaBody.DatiPagamento.DettaglioPagamento();
            dp.IBAN = new string('n', max + 1);
            Assert.IsTrue(dp.Error.Contains("IBAN"));
            dp = new FatturaElettronica.FatturaElettronicaBody.DatiPagamento.DettaglioPagamento();
            dp.IBAN = new string('n', min - 1);
            Assert.IsTrue(dp.Error.Contains("IBAN"));
        }

    }
}
