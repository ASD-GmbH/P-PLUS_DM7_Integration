using System;

namespace DM7_PPLUS_Integration.Auswahllisten
{
    public static class Auswahllisten_1
    {
        public static class Geschlecht
        {
            public static readonly Guid NichtAngegeben = new Guid("F9943FC4-5B2F-4DE7-BCAB-3E19501CAB89");
            public static readonly Guid Maennlich = new Guid("EA55105D-DDE7-48F6-A3D6-6059E6487681");
            public static readonly Guid Weiblich = new Guid("5FE792B5-6305-471F-A61C-D4A39C1FAFBF");
            public static readonly Guid Divers = new Guid("716464DA-9B64-4F22-A7C2-18E495A761AB");
        }

        public static class Konfession
        {
            public static readonly Guid Keine = new Guid("FD6B5DA4-E927-40AC-B0E3-BA08F448F089");
            public static readonly Guid altkatholische_Kirchensteuer = new Guid("63B14E9D-1B1F-4689-B7AF-DBDE97593805");
            public static readonly Guid evangelische_Kirchensteuer = new Guid("D1204BC1-7A1E-4122-A3AF-2A295C380BBC");
            public static readonly Guid freie_Religionsgemeinschaft_Alzey = new Guid("3A6697EA-60A2-41A3-9C5E-712F49C5FD88");
            public static readonly Guid Kirchensteuer_der_Freireligioesen_Landesgemeinde_Baden = new Guid("7E6E5018-FAA3-4B0C-988A-1F3B96FB76AE");
            public static readonly Guid freireligioese_Landesgemeinde_Pfalz = new Guid("AD5A9BB7-BAC4-471C-92F5-B3DDBAFF4A17");
            public static readonly Guid freireligioese_Gemeinde_Mainz = new Guid("B80EDB5E-5636-423D-85CA-35FEB9CB7594");
            public static readonly Guid franzoesisch_reformiert = new Guid("3F575BDB-8957-48FC-BE5E-E5586DD5A180");
            public static readonly Guid freireligioese_Gemeinde_Offenbach_Mainz = new Guid("FAA5FCDB-81DE-462B-A9C4-0C7D649A405E");
            public static readonly Guid griechisch_orthodox = new Guid("4B28506E-82EA-41FD-B2D9-E000CC2CFB17");
            public static readonly Guid Kirchensteuer_der_Israelitischen_Religionsgemeinschaft_Baden = new Guid("70784A1A-33F9-4F9F-AC9E-9A4B47C1D60E");
            public static readonly Guid israelitische_Kultussteuer_der_Kultusberechtigten_Gemeinden = new Guid("31F9A12B-8299-4CE0-9E6B-6D6DB6E4C8F6");
            public static readonly Guid israelitische_Kultussteuer = new Guid("A4F07FA9-4B4B-4C1B-A0D7-F2B2C68FFC1A");
            public static readonly Guid Kirchensteuer_der_Israelitischen_Religionsgemeinschaft_Wuerttemberg = new Guid("4C1A359F-E713-4455-9565-69DCDE4C613B");
            public static readonly Guid juedische_Kultussteuer = new Guid("B28AAF8D-AFB8-46A6-8D99-9F800C66FF13");
            public static readonly Guid evangelisch_lutherisch = new Guid("C5C5C05B-1F00-48BA-8A99-B7CFB65DE2F5");
            public static readonly Guid muslimisch = new Guid("8B30D527-DBCA-4976-AF6D-60308D340ED4");
            public static readonly Guid evangelisch_reformiert = new Guid("FD2D64AC-B5ED-499D-B9E7-A0FF8E2264A8");
            public static readonly Guid roemisch_katholische_Kirchensteuer = new Guid("3590D8DB-E01F-4839-844F-4FEAECEE2E9F");
            public static readonly Guid russisch_orthodox = new Guid("F7ECCF49-32D9-458A-B44A-F7979EA93EC9");
            public static readonly Guid unitarische_Religionsgemeinschaft_freie_Protestanten = new Guid("26A2D98E-93C0-4E8E-AE67-2811D86F759B");
        }

        public static class Titel
        {
            public static readonly Guid Kein = new Guid("56366E85-A1B5-405B-AEE8-658CBC15522A");
            public static readonly Guid Dr = new Guid("04230C6B-DFD9-4150-95CA-AC5B17F6F1A5");
            public static readonly Guid Prof = new Guid("EDA4C790-003C-47A0-9F6B-F49248DBB84B");
            public static readonly Guid Prof_Dr = new Guid("9ED37996-9559-4820-9741-CFD228EB49FA");
        }

        public static class Familienstand
        {
            public static readonly Guid Unbekannt = new Guid("DAE4C7C1-C604-4A1C-ADC4-679C7B5A759A");
            public static readonly Guid Ledig = new Guid("A7C07880-2999-4E3A-84E9-1D9064309E42");
            public static readonly Guid Verheiratet = new Guid("4AEF80E5-BD84-4E0E-9CC7-A2FB8B49FED3");
            public static readonly Guid Geschieden = new Guid("5FD3F26E-0EBF-4849-8F74-9DC732A43B23");
            public static readonly Guid Verwitwet = new Guid("BEA1E1B5-1B8A-42C5-A912-3E49B53364BA");
            public static readonly Guid Verpartnert = new Guid("BE67F390-0BB8-475E-BE34-79FA766B1D0B");
            public static readonly Guid Entpartnert = new Guid("A27892C1-D678-4FFE-B727-64395EE9E59A");
            public static readonly Guid Partnerhinterbliebend = new Guid("E6E918D5-67D8-4893-9D1C-793CA591DC5A");
            public static readonly Guid Getrennt_lebend = new Guid("45296DFD-485C-4B5D-B2FB-CE42669ED83E");
        }

        public static class Kontaktart
        {
            public static readonly Guid Telefon = new Guid("320FECBD-6194-4B9C-A4A6-D5FB60E10EDD");
            public static readonly Guid Email = new Guid("C5092F68-6048-4A1C-A7A9-413A391E665A");
            public static readonly Guid Fax = new Guid("453AE94F-FC2E-4D20-9D6B-8CEA8B5777BC");
            public static readonly Guid Web = new Guid("B7103AE2-62FC-4A9C-B0BF-8F65725E6428");
            public static readonly Guid Funkruf = new Guid("799EE5AA-6CDD-4954-818C-A3786C8BDD76");
            public static readonly Guid Zusaetzlich = new Guid("1954615F-F489-4FF7-B649-7998A10DEF16");
        }

        public static class Kontaktform
        {
            public static readonly Guid Privat = new Guid("45078825-F378-4764-B341-5DB31C8BA3FC");
            public static readonly Guid Geschaeftlich = new Guid("76E9CF9E-518C-406C-BF84-DD3E1063498B");
            public static readonly Guid Bereitschaft = new Guid("65ACAABB-34F0-4787-ACCD-E7F46F17F2D4");
        }
    }
}