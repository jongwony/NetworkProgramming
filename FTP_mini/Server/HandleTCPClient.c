#include<stdio.h>           // printf() fprintf()
#include<sys/socket.h>      // recv(), send()
#include<unistd.h>          // close()
#include<string.h>          // memset()
#include<dirent.h>	    // search directory
#include<stdlib.h>	    // atoi()
#include"sftpHdr.h"

FILE *fp;
DIR *dir_info;

void HandleLogfile(char *echoBuffer);   // create log file function
void DieWithError(char *errorMessage); // error handling function

void HandleTCPClient(int clntSock){
    int recvMsgSize;                    // size of received message
    int recvFileSize;                   // buffer size received message
    int origFileSize;                   // total size of file
    
    struct dirent *dir_entry;		// directory entry structure
    char *entryname;

    // initialized buffer
    memset(&fileName, 0, sizeof(fileName));
    memset(&echoBuffer, 0, sizeof(echoBuffer));
    memset(&fileBuf, 0, sizeof(fileBuf));
    memset(&fileSize, 0, sizeof(fileSize));
    memset(&dir_entry, 0, sizeof(dir_entry));

    recv(clntSock, &MsgType, 1, 0);     // receive ack msg

    printf("MsgType: %d\n",MsgType);
    
    /////////// rls COMMAND ////////////
    if(MsgType==rlsReq){
	//send ack msg
	MsgType = rlsAck;
	send(clntSock, &MsgType, 1, 0);

	// open server directory
	dir_info = opendir(".");
	if(dir_info!=NULL){

		// all item of directory
		while((dir_entry=readdir(dir_info))!=NULL){
			// send client
			entryname = dir_entry->d_name;
			if(send(clntSock, entryname, ENTRYSIZE, 0)!=ENTRYSIZE){
				DieWithError("send() dir entry error");
			}
		}
		closedir(dir_info);
    	}
    }
    /////////// rcd cOMMAND ////////////
    if(MsgType==rcdReq){
	    MsgType=rcdAck;
	    send(clntSock, &MsgType, 1, 0);

	    // recv directory name
	    recv(clntSock, diritem, ENTRYSIZE, 0);

	    // change directory
	    chdir(diritem);
	    dir_info=opendir(".");
	   
	    while((dir_entry=readdir(dir_info))!=NULL){
		// send client
		    entryname = dir_entry->d_name;
		    if(send(clntSock, entryname, ENTRYSIZE, 0)!=ENTRYSIZE){
			    DieWithError("send() dir entry error");
		    }
	    }
	    closedir(dir_info);
    }

    
    /////////// UPLOAD CASE ////////////
    if(MsgType==FileUpReq){                   
        // Receive file name
        if((recvMsgSize=recv(clntSock, fileName, FILENAME,0))<0){
            DieWithError("recv() failed: file name");
        }else{
            printf("\nfile name: %s\n", fileName);
        }

	// send ack msg for data merge exception
	send(clntSock, &MsgType, 1, 0);
       
	// Receive file size
        if((recvMsgSize=recv(clntSock, fileSize, FILESIZE, 0))<0){
            DieWithError("recv() failed: file size");
        }else{
            printf("file size: %s\n",fileSize);
            // if file size received, send ack message
            MsgType=FileUpAck;
            send(clntSock, &MsgType, 1, 0);
	    printf("Reception success!\n");
        }    
        origFileSize = atoi(fileSize);
        
        // receive file contents from client
        fp = fopen("result.bin", "wb");     // binary file
        if(fp==NULL){
            DieWithError("File open error");
        }
        
        recvFileSize=0;             // receive as BUFSIZE
        while(origFileSize>recvFileSize){
        // see if there is more data to receive
            if((origFileSize-recvFileSize)<BUFSIZE){        // near by EOF size
                if((recvMsgSize=recv(clntSock, fileBuf, origFileSize-recvFileSize,0))<0){
                    DieWithError("recv() failed");
                }
                fwrite(fileBuf, sizeof(char), origFileSize-recvFileSize,fp);
                recvFileSize += recvMsgSize;
                printf("recvFileSize=%d\n",recvFileSize);
            }
            else{
                if((recvMsgSize=recv(clntSock, fileBuf, BUFSIZE, 0))<0){
                    DieWithError("recv() failed");
                }
                fwrite(fileBuf, sizeof(char), BUFSIZE, fp);  
                recvFileSize += recvMsgSize;     
            }
        } 
        fclose(fp);
    }
    /////////// DOWNLOAD CASE ////////////
    if(MsgType==FileDownReq){
	    // recv file name
	    if((recvMsgSize = recv(clntSock, fileName, FILENAME,0))<0){
		    DieWithError("recv() failed: file name");
	    }
	    printf("\nfile name : %s\n", fileName);

	    // file name compare
	   fp=fopen(fileName, "rb");
	   if(fp==NULL){
		   DieWithError("match failed: file name");
	   }

	   // configure file size
	   fseek(fp, 0, SEEK_END);	// from 0 to SEEK_END
	   origFileSize = ftell(fp);	// long -> int
	   rewind(fp);			// cursor rewind
	   sprintf(fileSize, "%d", origFileSize);	// itoa
	   printf("file size %s\n", fileSize);

	   // send file size ack msg
	   if(send(clntSock, fileSize, FILESIZE, 0)!=FILESIZE){
		   DieWithError("send() wring byte sent file size");
	   }
	   printf("%s byte\n", fileSize);

	   // if server recv ack msg,
	   if(recv(clntSock, &MsgType, 1, 0)<=0){
		   DieWithError("ACK recv() failed");
	   }
	   printf("Reception success!\n");

	   // sending file
	   if(MsgType==FileDownAck){
		   while(!feof(fp)){
			   // position indicator auto read
			   fread(fileBuf, sizeof(char), BUFSIZE, fp);
			   if(send(clntSock, fileBuf, BUFSIZE, 0)!=BUFSIZE){
				   DieWithError("file send() failed!");
			   }
		   }
	   }
	   printf("send complete.\n");
	   fclose(fp);    
    }
    ////////////// ECHO CASE ////////////
    if(MsgType==EchoReq){          
        MsgType=EchoAck;
        send(clntSock, &MsgType, 1, 0); //send ack message
        
        if((recvMsgSize = recv(clntSock, echoBuffer, RCVBUFSIZE, 0))<0){
            DieWithError("recv() failed");
        }
        
        echoBuffer[recvMsgSize]='\0';   // end of buffer
        
	HandleLogfile(echoBuffer);      // append log file
        
	printf("%s\n", echoBuffer);     // print message at server
        
        // Send received string and receive again until end of transmission 
        while(recvMsgSize>0){           // zero indicates end of transmission
            // echo message back to client
            if(send(clntSock, echoBuffer, recvMsgSize, 0)!=recvMsgSize){
                DieWithError("send() failed");
            }
            // see if there is more data to receive
            if((recvMsgSize = recv(clntSock, echoBuffer, RCVBUFSIZE, 0))<0){
                DieWithError("recv() failed");
            }
        }
       
    }
    close(clntSock);       // close client socket
}
