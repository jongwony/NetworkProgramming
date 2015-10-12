#include<stdio.h>           // printf(), fprintf()
#include<stdlib.h>          // atoi(), exit()
#include<sys/socket.h>      // socket(), bind(), accept(), listen()
#include<arpa/inet.h>       // sockaddr_in, inet_addr()
#include<string.h>          // memset()
#include<unistd.h>          // close()
#include"sftpHdr.h"

#define MAXPENDING 5        // maximum outstanding connection requests

void DieWithError(char *errorMessage);      // error handling function
void HandleTCPClient(int clntSocket);       // TCP client handling function

int main(int argc, char *argv[]){
    struct sockaddr_in servAddr;    // local address
    struct sockaddr_in clntAddr;    // client address
    unsigned short servPort;        // server port
    unsigned int clntLen;           // length of client address data structure
    
    if(argc != 2){
        fprintf(stderr, "Usage: %s <Server Port>\n", argv[0]);
        exit(1);
    }
    
    servPort = atoi(argv[1]);       // first arg: local port
    
    // Create socket for incoming connections
    if((servSock = socket(PF_INET, SOCK_STREAM, IPPROTO_TCP))<0){
        DieWithError("socket() failed");
    }
    
    // Construct local address structure
    memset(&servAddr, 0, sizeof(servAddr));         // zero out structure
    servAddr.sin_family = AF_INET;                  // internet address family
    servAddr.sin_addr.s_addr = htonl(INADDR_ANY);   // any incoming interface
    servAddr.sin_port = htons(servPort);    // local port
    
    // Bind to the local address
    if(bind(servSock, (struct sockaddr *)&servAddr, sizeof(servAddr))<0){
        DieWithError("bind() failed");
    }
    
    // Mark the socket so it will listen for incoming connections
    if(listen(servSock, MAXPENDING<0)){
        DieWithError("listen() failed");
    }
   
    printf("Activated!\n");

    for(;;){        // run forever
        // set the size of the in-out parameter
        clntLen = sizeof(clntAddr);
        
        // wait for a client to connect
        if((clntSock = accept(servSock, (struct sockaddr *)&clntAddr, &clntLen))<0){
            DieWithError("accept() failed");
        }
        
        // client socket is connected to a client!
        printf("From %s : ", inet_ntoa(clntAddr.sin_addr));
        HandleTCPClient(clntSock);
    }
    // NOT REACHED    
}
