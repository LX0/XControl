﻿using System;
using System.Collections.Generic;
using System.Text;

using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Web.UI;
using System.Drawing;
using System.Globalization;
using System.Collections;

namespace XControl
{
    /// <summary>
    /// 价格输入控件，只能输入数字，通常只作为输入价格时候使用
    /// </summary>
    [Description("价格输入控件")]
    [ToolboxData("<{0}:DecimalBox runat=server></{0}:DecimalBox>")]
    [ToolboxBitmap(typeof(TextBox))]
    public class DecimalBox : TextBox
    {
        /// <summary>
        /// 小数点右边位数
        /// </summary>
        [Description("小数点右边精度值（默认为2位）")]
        [DefaultValue(2)]
        public Int32 CurrencyDecimalDigits
        {
            get
            {
                String num = (String)ViewState["CurrencyDecimalDigits"];
                if (String.IsNullOrEmpty(num)) return 0;
                Int32 k = 0;
                if (!Int32.TryParse(num, out k)) return 0;
                return k;
            }
            set
            {
                if (value == null)
                {
                    ViewState["CurrencyDecimalDigits"] = "2";
                }
                ViewState["CurrencyDecimalDigits"] = value.ToString();
            }
        }

        /// <summary>
        /// 小数点左边部分每组数字位数
        /// </summary>
        [Description("小数点部分每一组位数（如果多重分组使用逗号分隔）")]
        public String CurrencyGroupSizes
        {
            get
            {
                String num = (String)ViewState["CurrencyGroupSizes"];
                if (String.IsNullOrEmpty(num)) return null;
                return num;
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    ViewState.Remove("CurrencyGroupSizes");
                    return;
                }
                ViewState["CurrencyGroupSizes"] = value;
            }
        }

        /// <summary>
        /// 小数点左边部分每组数字分组符
        /// </summary>
        [Description("小数点左边部分每组数字分组符")]
        [DefaultValue(",")]
        public String CurrencyGroupSeparator
        {
            get
            {
                String str = (String)ViewState["CurrencyGroupSeparator"];
                if (String.IsNullOrEmpty(str)) return null;
                return str;
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    ViewState.Remove("CurrencyGroupSeparator");
                    return;
                }
                ViewState["CurrencyGroupSeparator"] = value;
            }
        }

        /// <summary>
        /// 获取或设置用作货币符号的字符串
        /// </summary>
        [Description("获取或设置用作货币符号的字符串")]
        [DefaultValue("￥")]
        public String CurrencySymbol
        {
            get
            {
                String symbol = (String)ViewState["CurrencySymbol"];
                if (String.IsNullOrEmpty(symbol))
                {
                    symbol="￥";
                }
                return symbol;
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    ViewState["CurrencySymbol"] = "￥";
                }
                ViewState["CurrencySymbol"] = value;
            }
        }

        /// <summary>
        /// 初始化价格输入控件的样式
        /// </summary>
        public DecimalBox()
            : base()
        {
            this.ToolTip = "只能输入数字价格！";
            BorderWidth = Unit.Pixel(0);
            BorderColor = Color.Red;
            BorderStyle = BorderStyle.Solid;
            Font.Size = FontUnit.Point(10);
            Width = Unit.Pixel(70);
            if (String.IsNullOrEmpty(Attributes["style"])) this.Attributes.Add("style", "border-bottom-width:1px;text-align : right ");
        }

        /// <summary>
        /// 已重载
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            //校验脚本
            this.Attributes.Add("onkeypress", "return ValidReal();");
            this.Attributes.Add("onblur", "return VaildDecimal1();");
            this.Page.ClientScript.RegisterClientScriptResource(typeof(NumberBox), "XControl.TextBox.Validator.js");
        }

        /// <summary>
        /// 当前值
        /// </summary>
        [Category("专用属性"), DefaultValue(0), Description("当前值")]
        public Decimal Value
        {
            get
            {
                if (String.IsNullOrEmpty(Text)) return Decimal.Zero;
                Decimal d = 0;

                if (!Decimal.TryParse(Text, out d)) return Decimal.Zero;
                //if (!Decimal.TryParse(Text, out d)) return Decimal.Zero;

                return d;
            }
            set
            {
                Int32[] intArray = new Int32[] { };
                NumberFormatInfo nf = new NumberFormatInfo();

                if (!String.IsNullOrEmpty(CurrencyGroupSizes))
                {
                    try
                    {
                        String[] strArray = CurrencyGroupSizes.Split(',');
                        ArrayList list = new ArrayList();

                        foreach (var item in strArray)
                        {
                            Int32 i = Int32.Parse(item);
                            list.Add(i);
                        }

                        intArray = (Int32[])list.ToArray(typeof(Int32));
                    }
                    catch (Exception)
                    {
                        throw new Exception("请检查分组输入！");
                    }
                    nf.CurrencyGroupSizes = intArray;
                }

                nf.CurrencyDecimalDigits = CurrencyDecimalDigits;
                if (!String.IsNullOrEmpty(CurrencyGroupSeparator))
                    nf.CurrencyGroupSeparator = CurrencyGroupSeparator;
                nf.CurrencySymbol = CurrencySymbol;

                Text = value.ToString("c", nf);
            }
        }
    }
}