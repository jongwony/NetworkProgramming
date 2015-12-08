#include<stdio.h>
#include<sys/socket.h>
#include<string.h>
#include"sftpHdr.h"

void DieWithError(char *errorMessage);

void ClientEchoHandle(int clntSock, char *echoString){
    
    int echoStringLen;         // length of string to echo
    int bytesRcvd, totalBytesRcvd;      // bytes read in single recv() and total bytes read
    
    //fgets(echoString, RCVBUFSIZE, stdin);   // standard input to send msg
    echoStringLen = strlen(echoString);     // determine input length

    // Send the string to the server
    if(send(clntSock, echoString, echoStringLen, 0) != echoStringLen){
        DieWithError("send() sent a different number of bytes than expected");
    }
    
    // Receive the same string back from the server
    totalBytesRcvd = 0;
    while(totalBytesRcvd < echoStringLen){
        /* Receive up to the buffer size
         * (-1 to leave space for a null terminator)
         * bytes from the sender */
         if((bytesRcvd = recv(clntSock, echoBuffer, RCVBUFSIZE-1, 0))<=0){
             DieWithError("recv() failed or connection closed prematurely");
         }
         totalBytesRcvd += bytesRcvd;   // keep tally of total bytes
         echoBuffer[bytesRcvd]='\0';    // terminate the string!
         printf("%s\n",echoBuffer);
    }
}
