#include<stdio.h>
#include<sys/socket.h>
#include<string.h>
#include<stdlib.h>
#include"sftpHdr.h"

FILE *fp;

void DieWithError(char *errorMessage);
void SendingAnime(int incnum);

void ClientDownloadHandle(int clntSock, char *fileName){
	int recvMsgSize;
	int origFileSize;
	int recvFileSize;
	int incnum =1;

	unsigned int fileNameLen;

	// initialize
	memset(&fileSize,0,sizeof(fileSize));
	memset(&fileBuf,0,sizeof(fileBuf));
	fileNameLen=strlen(fileName);

	// send file name
	if(send(clntSock, fileName, fileNameLen, 0)<0){
		DieWithError("send() failed: file name");
	}
	printf("file name : %s\n", fileName);

	// recv file size
	if((recvMsgSize=recv(clntSock, fileSize, FILESIZE, 0))<0){
		DieWithError("recv() failed: file size");
	}else{
		printf("file size: %s\n", fileSize);
		// if file size recv, send ack message
		MsgType=FileDownAck;
		send(clntSock, &MsgType, 1, 0);
	}
	origFileSize = atoi(fileSize);
	printf("file size : %d\n", origFileSize);

	// receive file contents from client
	fp = fopen("Download_file.bin","wb");	// binary file
	if(fp==NULL){
		DieWithError("File open error");
	}

	recvFileSize = 0;			// recv as BUFSIZE
	while(origFileSize>recvFileSize){
		//see if there is more data to recv
		if((origFileSize-recvFileSize)<BUFSIZE){	// near by EOF size
			if((recvMsgSize=recv(clntSock, fileBuf, origFileSize-recvFileSize,0))<0){
				DieWithError("recv() failed");
			}
			fwrite(fileBuf, sizeof(char), origFileSize-recvFileSize, fp);
			recvFileSize += recvMsgSize;
		}
		else{
			if((recvMsgSize=recv(clntSock, fileBuf, BUFSIZE, 0))<0){
				DieWithError("recv() failed");
			}
			fwrite(fileBuf, sizeof(char), BUFSIZE, fp);
			recvFileSize += recvMsgSize;
		}

		// sending progress animation
		SendingAnime(incnum);
		incnum++;
	}	
	printf("Download complete.\n");
	fclose(fp);
}
