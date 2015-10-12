#include<stdio.h>
#include<sys/socket.h>
#include<string.h>
#include"sftpHdr.h"

FILE *fp;

void DieWithError(char *errorMessage);
void SendingAnime(int incnum);

void ClientFileHandle(int clntSock, char *recvfileName){
    
    unsigned int fileNameLen;
    int origFileSize;
    int incnum=1;

    // initialize
    memset(&fileSize,0,sizeof(fileSize));
    memset(&fileBuf, 0, sizeof(fileBuf));
    
    fp = fopen(recvfileName, "rb");
    
    if(fp==NULL){
        DieWithError("File open error");
    }
    
    fileNameLen = strlen(recvfileName);    // file name size
    
    // file size seeking
    fseek(fp, 0, SEEK_END);     // from 0 to SEEK_END
    origFileSize = ftell(fp);       // long -> int
    rewind(fp);                 // cursor rewind!!
    sprintf(fileSize, "%d", origFileSize);  // itoa substitute
    
    // send the file name
    if(send(clntSock, recvfileName, fileNameLen, 0)!=fileNameLen){
        DieWithError("send() wrong byte sent file name");
    }else{
        printf("file name %s\n", recvfileName);
    }
   
    recv(clntSock, &MsgType, 1, 0);

    // send the file size
    if(send(clntSock, fileSize, FILESIZE, 0)!=FILESIZE){
        DieWithError("send() wrong byte sent file size");
    }else{
        printf("%s byte\n", fileSize);
    }
    
    // if server file size ack msg send, send file contents.
    if((recv(clntSock, &MsgType, 1, 0))<=0){
        DieWithError("ACK recv() failed");
    }else{
        printf("ACK message received!\n");
    }
    
    if(MsgType==FileUpAck){
        // send file contents
        while(!feof(fp)){
            fread(fileBuf, sizeof(char), BUFSIZE, fp); //position indicator auto read
            if(send(clntSock, fileBuf, BUFSIZE, 0)!=BUFSIZE){
                DieWithError("send() failed");
            }

	    // sending progress animation
	    SendingAnime(incnum);
	    incnum++;
        } 
    }else{          // validate ACK msg 
        DieWithError("ACK recv() corrupted");
    }

    printf("send complete.\n");
    fclose(fp);
}
