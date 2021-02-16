using Android.Content;
using Android.Database;
using Android.Provider;
using System;
using System.Collections.Generic;

namespace Task2
{
    public class LogVerileri
    {
        public static string SMS_TURU = "";
        public class Kayit
        {
            public string Numara { get; set; }
            public string Isim { get; set; }
            public string Tarih { get; set; }
            public string Tip { get; set; }
            public string Durasyon { get; set; }
        }
        public class SMS
        {
            public string Gonderen { get; set; }
            public string Icerik { get; set; }
            public string Tarih { get; set; }
            public string Isim { get; set; }
        }
        public class Isimler
        {
            public string Isim { get; set; }
            public string Numara { get; set; }
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
        public string tur(string input)
        {
            try
            {
                return donusum[input];
            }
            catch (Exception ex) { return ex.Message; }
        }
        public static DateTime suankiZaman(long yunix)
        {
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime date = start.AddMilliseconds(yunix).ToLocalTime();
            return date;
        }
        public string durasyon(string input)
        {
            TimeSpan taym = TimeSpan.FromSeconds(Convert.ToDouble(input));
            return taym.ToString(@"hh\:mm\:ss");
        }
        public void aramaKayitlariniCek()
        {
            try
            {
                Android.Net.Uri uri = CallLog.Calls.ContentUri;
                string[] neleriAlicaz = {
            CallLog.Calls.Number,
            CallLog.Calls.CachedName,
            CallLog.Calls.Date,
            CallLog.Calls.Duration,
            CallLog.Calls.Type
        };
                CursorLoader c_loader = new CursorLoader(activity, uri, neleriAlicaz, null, null, null);
                ICursor cursor = (ICursor)c_loader.LoadInBackground();
                bool isFirst = cursor.MoveToFirst();
                if (isFirst)
                {
                    do
                    {
                        string isim = "Kayıtsız numara";
                        try
                        {
                            isim =
                        cursor.GetString(cursor.GetColumnIndex(CallLog.Calls.CachedName)).ToString();
                        }
                        catch (Exception) { }
                        kayitlar.Add(new Kayit
                        {
                            Tarih = suankiZaman(long.Parse(cursor.GetString(cursor.GetColumnIndex(CallLog.Calls.Date)))).ToString(),
                            Numara = cursor.GetString(cursor.GetColumnIndex(CallLog.Calls.Number)).ToString(),
                            Isim = isim,
                            Durasyon = durasyon(cursor.GetString(cursor.GetColumnIndex(CallLog.Calls.Duration))),
                            Tip = tur(cursor.GetString(cursor.GetColumnIndex(CallLog.Calls.Type))),
                        });
                    } while (cursor.MoveToNext());
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
                        while (phones.MoveToNext())
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
                            catch (Exception)
                            {

                            }
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
                Android.Net.Uri uri = (neresi_ == "gelen")
                ? Telephony.Sms.Inbox.ContentUri : Telephony.Sms.Sent.ContentUri;

                SMS_TURU = (neresi_ == "gelen")
                ? "Incoming SMS" : "Outgoing SMS";

                string[] neleriAlicaz = {
            "body", "date", "address"
        };

                CursorLoader c_loader = new CursorLoader(activity, uri, neleriAlicaz, null, null, null);
                ICursor cursor = (ICursor)c_loader.LoadInBackground();
                bool isFirst = cursor.MoveToFirst();
                if (isFirst)
                {
                    do
                    {
                        string isim = "Not found";
                        try
                        {
                            isim = getContactbyPhoneNumber(MainActivity.global_activity, cursor.GetString(cursor.GetColumnIndex("address")));
                        }
                        catch (Exception) { }
                        smsler.Add(new SMS
                        {
                            Gonderen = cursor.GetString(cursor.GetColumnIndex("address")),
                            Icerik = cursor.GetString(cursor.GetColumnIndex("body")),
                            Tarih = suankiZaman(long.Parse(cursor.GetString(cursor.GetColumnIndex("date")))).ToString(),
                            Isim = isim
                        });
                    } while (cursor.MoveToNext());
                }
            }
            catch (Exception) { }
        }
        public string getContactbyPhoneNumber(Context c, string phoneNumber)
        {
            try
            {
                Android.Net.Uri uri = Android.Net.Uri.WithAppendedPath(ContactsContract.PhoneLookup.ContentFilterUri, (phoneNumber));
                string[] projection = { ContactsContract.Contacts.InterfaceConsts.DisplayName };
                ICursor cursor = c.ContentResolver.Query(uri, projection, null, null, null);
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
            catch (Exception) { return "Not found"; }
        }
    }
}