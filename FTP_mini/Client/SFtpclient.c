#include<stdio.h>       // for printf() fprintf()
#include<sys/socket.h>  // for socket(), connect(), send(), recv()
#include<arpa/inet.h>   // for sockaddr_in, inet_addr()
#include<stdlib.h>      // for atoi(), exit()
#include<string.h>      // for memset()
#include<unistd.h>      // for close()
#include<stdio_ext.h>	// for __fpurge()
#include"sftpHdr.h"

void DieWithError(char *errorMessage);  // error handling function
void ClientEchoHandle(int clntSocket, char *echoString);  // echo handle
void ClientFileHandle(int clntSocket, char *fileName);  // file handle
void ClientDownloadHandle(int clntSocket, char *fileName); // Download handle

int main(int argc, char *argv[]){
    char *servIP;           // server IP address(dotted quad)
    struct sockaddr_in servAddr;	// server address
    unsigned short servPort;		// server port

    char *operation;        // client 203.252.164.144 5000 
    char *operend;          // file name or echo message
    int recvMsgSize;

    if((argc<2)||(argc>3)){ // test for correct number of arguments
        fprintf(stderr, "Usage: %s <ServerIP> [<Echo Port>]\n", argv[0]);
        exit(1);
    }
    servIP = argv[1];       // first arg: server IP address(dotted quad)
    
    if(argc==3){
        servPort = atoi(argv[2]);   // use given port, if any
    }else{
        servPort = 7;   // 7 is the well-known port for the echo service
    }

    printf("Welcome to Simple FTP!\n");

  while(1){
    // Create a reliable, stream socket using TCP
    if((clntSock = socket(PF_INET, SOCK_STREAM, IPPROTO_TCP))<0){
        DieWithError("socket() failed");
    }
    
    // Construct the server address structure
    memset(&servAddr, 0, sizeof(servAddr));     // zero out structure
    servAddr.sin_family = AF_INET;                  // internet address family
    servAddr.sin_addr.s_addr = inet_addr(servIP);   // server IP address
    servAddr.sin_port = htons(servPort);        // server port
    
    // Initialize command pointer
    operation = (char *)malloc(sizeof(char)*COMMANDSIZE);
    operend = (char *)malloc(sizeof(char)*COMMANDSIZE);

    // Establish the connection to the echo server
    if(connect(clntSock, (struct sockaddr *) &servAddr, sizeof(servAddr))<0){
        DieWithError("connect() failed");
    }

    // standard input
    printf("FTP>");
    __fpurge(stdin);
    scanf("%s",operation);

    // exit or e
    if(!strcmp(operation,"exit")||!strcmp(operation,"e")){
	    break;
    }
    // ls or l
    if(!strcmp(operation, "ls")||!strcmp(operation,"l")){
	system("ls");
    }
    // rls
    if(!strcmp(operation, "rls")){
	MsgType = rlsReq;
	send(clntSock, &MsgType, 1, 0);
	
	// recv ack msg
	recv(clntSock, &MsgType, 1, 0);

	while(1){
		if((recvMsgSize=recv(clntSock, diritem, ENTRYSIZE, 0))<0){
			DieWithError("recv() failed");
		}
		if(recvMsgSize==0){
			break;
		}
		printf("%s\n",diritem);
	}
	printf("\n");
    }
    
    // change directory
    if(!strcmp(operation, "cd")){
	__fpurge(stdin);
	printf("Go to: ");
   	scanf("%s",operend);
	chdir(operend);
    }
    // change server directory 
    if(!strcmp(operation, "rcd")){
	MsgType = rcdReq;
	send(clntSock, &MsgType, 1, 0);

	//recv ack msg
	recv(clntSock, &MsgType, 1, 0);

	__fpurge(stdin);
	printf("Go to: ");
   	scanf("%s",operend);

	//send operend
	send(clntSock, operend, ENTRYSIZE, 0);

	while(1){
		if((recvMsgSize=recv(clntSock, diritem, ENTRYSIZE, 0))<0){
			DieWithError("recv() failed");
		}
		if(recvMsgSize==0){
			break;
		}
		printf("%s\n",diritem);
	}

    }
    // UPLOAD file to the server
    if(!strcmp(operation, "put")||!strcmp(operation, "p")){
        MsgType=FileUpReq;                      // FileUpReq 02
        send(clntSock, &MsgType, 1, 0);    
	printf("UPLOAD file name to %s: ", servIP);
	__fpurge(stdin);
   	scanf("%s",operend);
        ClientFileHandle(clntSock, operend);
    }
    // DOWNLOAD file from server
    if(!strcmp(operation, "get")||!strcmp(operation, "g")){
	MsgType=FileDownReq;			// FileDownReq 03
	send(clntSock, &MsgType, 1, 0);
	printf("DOWNLOAD file name from %s: ", servIP);
	__fpurge(stdin);
   	scanf("%s",operend);
	ClientDownloadHandle(clntSock, operend);
    }
    // ECHO msg
    if(!strcmp(operation, "echo")){
        MsgType=EchoReq;                        // EchoReq 01
        send(clntSock, &MsgType, 1, 0);         
        
        // Receive Ack msg from server
        if((recv(clntSock, &MsgType, 1, 0))<0){
            DieWithError("recv() failed");
        }
        
        if(MsgType==EchoAck){
            printf("ACK message received!\n");
            printf("Send to %s: ", servIP);
            // sending echo msg
            ClientEchoHandle(clntSock, operend); 
        }else{
            printf("ACK not received!\n");
        }
    }
    // memory free command pointer
    free(operation);
    free(operend);
    close(clntSock);
  }
    exit(0);
}
