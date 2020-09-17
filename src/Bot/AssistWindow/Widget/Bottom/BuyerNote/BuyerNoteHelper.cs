using System;
using System.Collections.Generic;
using System.Linq;
using Bot.Common;
using Bot.Common.Account;
using BotLib.Extensions;
using DbEntity;

namespace Bot.AssistWindow.Widget.Bottom.BuyerNote
{
    public class BuyerNoteHelper
    {
        public static BuyerNoteEntity GetNewestBuyerNote(string buyerMain, string seller)
        {
            BuyerNoteEntity buyerNote = null;
            var ets = GetBuyerNotesOnlyBuyer(buyerMain, seller);
            ets = ets.OrderByDescending(k => k.ModifyTick).ToList();
            if (!ets.xIsNullOrEmpty())
            {
                if (Params.BuyerNote.GetSetIsPreferSelfNote(seller))
                {
                    buyerNote = ets.FirstOrDefault(k => k.Recorder == seller);

                }
                if (buyerNote == null)
                {
                    buyerNote = ets.First();
                }
            }
            return buyerNote;
        }

        public static List<BuyerNoteEntity> SearchBuyerNotes(DateTime? dateFrom, DateTime? dateTo, string sellerCond, string buyerCond, string seller)
        {
            string dbAccount = GetDbAccount(seller);
            return DbHelper.Fetch<BuyerNoteEntity>(dbAccount, k =>
            {
                var rt = false;
                if (dateFrom.HasValue)
                {
                    rt = k.RecordTime >= dateFrom.Value;
                }
                if (dateTo.HasValue)
                {
                    rt = k.RecordTime <= dateTo.Value;
                }
                if (!string.IsNullOrEmpty(sellerCond))
                {
                    rt = k.Recorder.Contains(sellerCond);
                }
                if (!string.IsNullOrEmpty(buyerCond))
                {
                    rt = k.BuyerMainNick.Contains(buyerCond);
                }
                return rt;
            });
        }

        public static List<BuyerNoteEntity> GetBuyerNotesSellerAndBuyer(string buyerMain, string seller)
        {
            string dbAccount = GetDbAccount(seller);
            return DbHelper.Fetch<BuyerNoteEntity>(dbAccount, k => k.Recorder == seller && k.BuyerMainNick == buyerMain);
        }

        public static List<BuyerNoteEntity> GetBuyerNotes(string seller)
        {
            return DbHelper.Fetch<BuyerNoteEntity>(GetDbAccount(seller));
        }


        public static void Delete(FavoriteNoteEntity fEt)
        {
            fEt.IsDeleted = true;
            DbHelper.SaveToDb(fEt, true);
        }

        public static List<BuyerNoteEntity> GetBuyerNotesOnlyBuyer(string buyerMain, string seller)
        {
            string dbAccount = GetDbAccount(seller);
            return DbHelper.Fetch<BuyerNoteEntity>(dbAccount, k => k.BuyerMainNick == buyerMain);
        }

        public static string GetDbAccount(string seller)
        {
            return AccountHelper.GetPubDbAccount(seller);
        }

        public static void Delete(BuyerNoteEntity et)
        {
            et.IsDeleted = true;
            DbHelper.SaveToDb(et, true);
        }

        public static List<FavoriteNoteEntity> GetFavNotes(string dbAccount)
        {
            return DbHelper.Fetch<FavoriteNoteEntity>(dbAccount);
        }

        public static void BatchDelete(List<BuyerNoteEntity> delList)
        {
            if (!delList.xIsNullOrEmpty())
            {
                foreach (var k in delList)
                {
                    k.IsDeleted = true;
                }
                DbHelper.BatchSaveToDb(delList.ToArray());
            }
        }

        public static BuyerNoteEntity Create(string buyerMain, string seller, string note)
        {
            string dbAccount = GetDbAccount(seller);
            var et = EntityHelper.Create<BuyerNoteEntity>(dbAccount);
            et.BuyerMainNick = buyerMain;
            et.Recorder = seller;
            et.Note = (note ?? "");
            et.RecordTime = BatTime.Now;
            DbHelper.SaveToDb(et, true);
            return et;
        }

        public static void Update(string note, BuyerNoteEntity et)
        {
            et.Note = note;
            et.RecordTime = BatTime.Now;
            DbHelper.SaveToDb(et, true);
        }

    }
}
