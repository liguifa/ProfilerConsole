using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfilerConsole.View
{
    public class Console
    {
        public static string GetConsole()
        {
            StringBuilder console = new StringBuilder();
            console.AppendLine(GetConsoleStyle());
            console.AppendLine(GetConsoleHtml());
            return console.ToString();
        }

        private static string GetConsoleHtml()
        {
            return @"
<div class='profilerconsole-panel'>
    <div class='profilerconsole-panel-tabs'>
            <ul>
    
                <li class='profilerconsole-panel-tabs-active'>请求</li>
        </ul>
    </div>
    <div id = 'profilerconsole-request'>
            <table class='profilerconsole-table-head profilerconsole-table' border='0' cellspacing='0' >
                <thead class='profilerconsole-table-tbody'>
                    <tr>
                        <th class='profilerconsole-request-url'>Url</th>
                        <th class='profilerconsole-request-duration'>请求时间</th>
                        <th class='profilerconsole-request-start'>开始时间</th>
                        <th class='profilerconsole-request-end'>结束时间</th>
                    </tr>
                </thead>
                </table>
                <table class='profilerconsole-table-content profilerconsole-table' border='0' cellspacing='0'>
                    <tbody class='profilerconsole-table-tbody'>
          @{
                            foreach(var e in Model.Events)
                            {
                                <tr>
                                        <td class='profilerconsole-request-url'>@e.Name</td>
                                        <td class='profilerconsole-request-duration'>@{@e.Duration
    }
    ms</td>
                                        <td class='profilerconsole-request-start'>@e.StartTime.ToString(""yyyy-MM-dd hh:mm:ss"")</td>
                                        <td class='profilerconsole-request-end'>@e.EndTime.ToString(""yyyy-MM-dd hh:mm:ss"")</td>
                                    </tr>
                            }
}
                    </tbody>
            </table>
    </div>
</div>";
        }

        private static string GetConsoleStyle()
        {
            return @"<style>
                 /*滚动条样式开始*/
.profilerconsole-panel *::-webkit-scrollbar {  
  width: 14px;  
  height: 14px;  
}  
  
.profilerconsole-panel *::-webkit-scrollbar-track,  
.profilerconsole-panel *::-webkit-scrollbar-thumb {  
  border-radius: 999px;  
  border: 5px solid transparent;  
}  
  
.profilerconsole-panel *::-webkit-scrollbar-track {  
  box-shadow: 1px 1px 5px rgba(0,0,0,.2) inset;  
}  
  
.profilerconsole-panel *::-webkit-scrollbar-thumb {  
  min-height: 20px;  
  background-clip: content-box;  
  box-shadow: 0 0 0 5px rgba(0,0,0,.2) inset;  
}  
  
.profilerconsole-panel *::-webkit-scrollbar-corner {  
  background: transparent;  
}
    .profilerconsole-panel{
        position:fixed;
        bottom:0px;
        left: 0px;
        height:300px;
        width:100%;
        background-color:#ffffff;
        padding: 0px;
        margin: 0px;
    }
    .profilerconsole-panel-tabs{
        height: 42px;
        width: 100%;
        background-color: #EFEFF4;
        border-top:solid 1px #e6e6e6;
        border-bottom:solid 1px #e6e6e6;
    }
    .profilerconsole-panel-tabs ul{
        padding: 0px;
        margin: 0px;
        list-style: none;
    }
    .profilerconsole-panel-tabs ul li{
        float: left;
        width: 66px;
        height: 100%;
        line-height: 42px;
        text-align: center;
        font-size: 16px;
        font-family: Helvetica Neue,Helvetica,Arial,sans-serif;
        border-right: solid 1px #e6e6e6;
    }
    .profilerconsole-panel-tabs ul li:hover{
        background: #ffffff;
    }
    .profilerconsole-panel-tabs-active{
        background-color: #ffffff;
        cursor: default;
    }
    .profilerconsole-table{
        width: 100%;
        border:0px;
        overflow-y: scroll;
        width: 100%;
        display: inline-block;
    }
    .profilerconsole-table-tbody{
        width: 100%;
        display: inline-block;
    }
    .profilerconsole-table thead tr{
        height: 29px;
        background: #FBF9FE;
        border-bottom: solid 1px #e6e6e6;
        width: 100%;
        display: inline-block;
    }
    .profilerconsole-table tbody tr{
        background: #ffffff;
        height: 20px;
        display: inline-block;
        width: 100%;
    }
    .profilerconsole-table th{
        border-right: 1px solid #e6e6e6;
        text-align: left;
        padding-left: 10px;
        padding-top: 2px;
        padding-bottom: 2px;
        display: inline-block;
        margin-left: -5px;
    }
    .profilerconsole-table td{
        border-right: 1px solid #e6e6e6;
        background: #ffffff;
        padding-left: 10px;
        padding-top: 2px;
        padding-bottom: 2px;
        margin-left: -5px;
        line-height: 15px;
        height: 20px;
        display: inline-block !important;

    }
    .profilerconsole-request-url{
        width: 40%;
    }
    .profilerconsole-request-duration{
        width: 20%;
    }
    .profilerconsole-request-start{
        width: 20%;
    }
    .profilerconsole-table-content{
        max-height: 231px !important; 
    }
    .profilerconsole-table td{
        display: inline-block;
    }
    .profilerconsole-table-head::-webkit-scrollbar{
        color: #FBF9FE;
         background: #FBF9FE;
    }
    .profilerconsole-table-content tr:hover td,.profilerconsole-table-content tr:hover {
        background: #DDEEFF !important;
    }
    .profilerconsole-table-content tr:nth-child(even) td,.profilerconsole-table-content tr:nth-child(even) {
        background: #F5F5F5;
    }
            </style>";
        }
    }
}
