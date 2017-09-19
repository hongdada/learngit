using lpapi.rongzi.com.Models;
using Rongzi.Entity;
using Rongzi.Infrastructure.Constant;
using Rongzi.Infrastructure.Http;
using System;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;
using System.Web.Http;
using Rongzi.Cache.Redis;
using Rongzi.Infrastructure.Crypto;
using Rongzi.Entity.DTO.User;
using Rongzi.Infrastructure.Log;
using lpapi.rongzi.com.Config;
using Rongzi.Entity.Applicant;
using Rongzi.Infrastructure.Util;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Net.Http.Headers;
using lpapi.rongzi.com.Helpers;
using Rongzi.Entity.Enum;
using Rongzi.Entity.DTO.NewApplyInfo;
using Rongzi.Entity.Response.NApplyInfo;
using lpapi.rongzi.com.Services;
using System.Web.Http.Filters;
using lpapi.rongzi.com.SEO;
using System.Web;
using Rongzi.Entity.Response.Applicant;

namespace lpapi.rongzi.com.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [RoutePrefix("api/RegApply")]
    public class RegAndApplyApiController : BaseController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("OnekeyApply")]
        public ResponseContext<object> RegApply(RegApplyDto req)
        {
            var result = new ResponseContext<object>() { Head = new ResponseHead() };

            if (!ModelState.IsValid)
            {
                result.Head.Ret = -1;
                result.Head.Msg = "请检查填写内容";
                return result;
            }

            if (SmsHelper.NeedSmsCode4OneClickApply(req.CellPhoneNumber, req.RegisterSource))
            {
                if (string.IsNullOrWhiteSpace(req.SmsCode))
                {
                    result.Head.Ret = -1;
                    result.Head.Msg = "请填写验证码";
                    return result;
                }
                SmsHelper helper = new SmsHelper();
                var ret = helper.VerifySmsCode(req.CellPhoneNumber, req.SmsCode);
                if (ret.Code != ErrCode.Sucess)
                {
                    result.Head.Ret = -1;
                    result.Head.Msg = "验证码错误";
                    return result;
                }
            }

            var oRet = GetOneKeyApply(req);
            if (oRet == null || oRet.newApplyInfoId == Guid.Empty)
            {
                result.Head.Ret = -1;
                result.Head.Msg = "注册失败";
                return result;
            }
            if (!ProductService.PostCreateUpdateNApplyInfo(req, oRet))
            {
                result.Head.Ret = -1;
                result.Head.Msg = "更新需求书失败";
                return result;
            }
            var prdList = ProductService.PostEvaluate(oRet);
            if (prdList == null)
            {
                result.Head.Ret = -1;
                result.Head.Msg = "测评失败";
                return result;
            }
            var userMark = AccountService.PostUserSyntheticalMarkDto(oRet.UserId);
            if (userMark == null)
            {
                result.Head.Ret = -1;
                result.Head.Msg = "根据用户id获取对应的评分";
                return result;
            }
            if (prdList != null && prdList.Count > 0)
            {

            }

            result.Head.Ret = 0;
            result.Content = prdList;
            return result;
        }

        /// <summary>
        /// oneKeyApply
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        private static ReqApplyLogResponse GetOneKeyApply(RegApplyDto req)
        {
            var oRet = AccountService.GetRespForOneKeyApply(req.CellPhoneNumber, req.UserName, req.LoanMoney, req.Identity, req.RegisterSource, req.Gender, UtilityFunction.GetRemoteClientIP(HttpContext.Current.Request));
            if (oRet != null)
            {
                //设置产品列表删选条件
                //string cookieValue = CookieManager.getCookieValue("productList");
                //if (!string.IsNullOrEmpty(cookieValue))
                //{
                //    VmProdcutCookie curCookie = ObjCrypto.Decrypt(cookieValue, typeof(VmProdcutCookie)) as VmProdcutCookie;
                //    if (curCookie != null)
                //    {
                //        if ((curCookie.IDType == 1 || curCookie.IDType == 2)
                //            && (req.Identity == 4 || req.Identity == 8))
                //        {
                //            if (req.Identity == 4)
                //                req.Identity = 3;
                //            if (req.Identity == 8)
                //                req.Identity = 4;
                //            curCookie.BusinessYear = 0;
                //            curCookie.CompanyLocation = 0;
                //        }

                //        if ((curCookie.IDType == 3 || curCookie.IDType == 4)
                //            && (req.Identity == 1 || req.Identity == 2))
                //        {
                //            curCookie.IncomeDistributionType = 0;
                //            curCookie.Salary = 0;
                //            curCookie.SocialSecurityFund = 0;
                //            curCookie.WorkingAge = 0;
                //        }
                //        if (req.Identity == 4)
                //            req.Identity = 3;
                //        if (req.Identity == 8)
                //            req.Identity = 4;
                //        curCookie.IDType = req.Identity;
                //        curCookie.LoanAmount = req.LoanMoney;
                //        cookieValue = ObjCrypto.Encrypt(curCookie);
                //        CookieManager.setCookie("productList", cookieValue, DateTime.Now.AddMonths(1), ".rongzi.com", "/");
                //    }
                //}
                //else
                //{
                //    VmProdcutCookie vm = new VmProdcutCookie();
                //    if (req.Identity == 4)
                //        req.Identity = 3;
                //    if (req.Identity == 8)
                //        req.Identity = 4;
                //    vm.IDType = req.Identity;
                //    vm.LoanAmount = req.LoanMoney;
                //    string cookie = ObjCrypto.Encrypt(vm);
                //    CookieManager.setCookie("productList", cookie, DateTime.Now.AddMonths(1), ".rongzi.com", "/");
                //}

            }
            return oRet;
        }

    
    }
}
