import json
from plyer import notification
import os
import time
from smtplib import SMTP_SSL
from email.mime.text import MIMEText
from email.mime.multipart import MIMEMultipart
from email.header import Header


def sendmesssage(version, url) :
    host_server = 'smtp.163.com'  #qq邮箱smtp服务器
    sender_address = 'Ghost12345@163.com' #发件人邮箱
    pwd = 'MJHEWYMVTCYSPEBE'
    receiver = ['917749218@qq.com']#收件人邮箱
    mail_title = "New update for WSA! Version " + version + " is now available!" #邮件标题
    mail_content = "File Name: " + "MicrosoftCorporationII.WindowsSubsystemForAndroid_" + version + "_neutral_~_8wekyb3d8bbwe.Msixbundle\nURL: " + url #邮件正文内容
    # 初始化一个邮件主体
    msg = MIMEMultipart()
    msg["Subject"] = Header(mail_title,'utf-8')
    msg["From"] = sender_address
    # msg["To"] = Header("测试邮箱",'utf-8')
    msg['To'] = ";".join(receiver)
    # 邮件正文内容
    msg.attach(MIMEText(mail_content,'plain','utf-8'))
    smtp = SMTP_SSL(host_server) # ssl登录
    # login(user,password):
    # user:登录邮箱的用户名。
    # password：登录邮箱的密码，像笔者用的是网易邮箱，网易邮箱一般是网页版，需要用到客户端密码，需要在网页版的网易邮箱中设置授权码，该授权码即为客户端密码。
    smtp.login(sender_address,pwd)
    # sendmail(from_addr,to_addrs,msg,...):
    # from_addr:邮件发送者地址
    # to_addrs:邮件接收者地址。字符串列表['接收地址1','接收地址2','接收地址3',...]或'接收地址'
    # msg：发送消息：邮件内容。一般是msg.as_string():as_string()是将msg(MIMEText对象或者MIMEMultipart对象)变为str。
    smtp.sendmail(sender_address,receiver,msg.as_string())
    # quit():用于结束SMTP会话。
    smtp.quit()

dir = os.path.dirname(__file__)
with open(dir + "\\Data\\Windows Subsystem For Android\\archive_meta.json", "r") as f:
    readjson = f.read()
    f.close()
mainjson = json.loads(readjson)
checkedversion = list(mainjson.keys())[-1]
print("Checked version: " + checkedversion)
if os.path.exists(dir + "\\currentversion.json") == False:
    with open(dir + "\\currentversion.json", "w") as f:
        f.write("{\"WSAversion\": \"0.0.0.0\"}") 
        f.close
with open(dir + "\\currentversion.json", "r") as f:
    versionjson = f.read()
    f.close()
if versionjson == "" :
    versionjson = "{\"WSAversion\": \"0.0.0.0\"}"
subjson = json.loads(versionjson)
currentversion = subjson["WSAversion"]
print("Current version: " + currentversion)
if currentversion != checkedversion:
    if os.path.exists(dir + "\\WSAbetaurl.txt") == False:
        betaurl = "null"
    else :
        with open(dir + "\\WSAbetaurl.txt", "r") as f:
            betaurl = f.read()
            f.close()
    print("New update for WSA! Version " + checkedversion + " is now available!")
    sendmesssage(checkedversion, betaurl)
    notification.notify(
            title = "New update for WSA! Version " + checkedversion + " is now available!",
            message = betaurl,
            app_icon = dir + "\\wsa.ico",
            timeout = 5,
        )
    subjson["WSAversion"] = checkedversion
    with open(dir + "\\currentversion.json", "w") as f:
        json.dump(subjson, f)
        f.close()
else:
    print("No update for WSA!")
for i in range(60,0,-1):
    print(f'\rWill exit after {i} seconds.',end='')  #\r让光标回到行首 ，end=''--结束符为空，即不换行
    time.sleep(1) #让程序等待1秒