#include<stdio.h>
#include<sys/socket.h>
#include<arpa/inet.h>
#include<string.h>
#include<sys/time.h>
#include<sys/types.h>
#include<unistd.h>

#define MAXPENDING 4

int main(void){
	int servSock;
	int clntSock[MAXPENDING];
	int maxfd, i,j, recvSize;
	int clntNum=0;
	char buffer[256];

	struct sockaddr_in servAddr;
	unsigned short port = 20000;

	struct sockaddr_in clntAddr;
	socklen_t clntLen = sizeof(clntAddr);

	// select file descriptor
	fd_set readfd;

	// open socket
	servSock = socket(PF_INET, SOCK_STREAM, IPPROTO_TCP);
	if(servSock<0){
		perror("socket() failed");
	}
	// bind socket
	servAddr.sin_family = AF_INET;
	servAddr.sin_addr.s_addr = htonl(INADDR_ANY);
	servAddr.sin_port = htons(port);
	if(bind(servSock,(struct sockaddr*)&servAddr,sizeof(servAddr))<0){
		perror("bind() error");
	}

	// listen socket
	listen(servSock,MAXPENDING);

	// readfd bit set
	maxfd = servSock+1;

	printf("Server Active!\n");
	while(1){
		printf("loop 0-line\n");
		sleep(1);

		// set socket number
		maxfd = servSock + 1 + clntNum; 
		FD_ZERO(&readfd);
		FD_SET(servSock,&readfd);
		for(i=0;i<clntNum;i++){
			FD_SET(clntSock[i],&readfd);
			printf("clntSock[%d]\n",i);
		}

		// select socket
		printf("maxfd = %d\n",maxfd);
		select(maxfd,&readfd,NULL,NULL,NULL);

		// server socket selected
		if(FD_ISSET(servSock, &readfd)){
			// accept socket
			clntSock[clntNum++]=accept(servSock,(struct sockaddr*)&clntAddr,&clntLen);
			printf("client accept! total:%d\n",clntNum);
		}
		
		// client socket selected
		for(i=0;i<clntNum;i++){
			if(FD_ISSET(clntSock[i],&readfd)){
				if((recvSize = recv(clntSock[i],buffer,256,0))>0){
					buffer[recvSize]='\0';
					printf("msgsize = %d\n",recvSize);
				}else{
					// connection failed!
					printf("clntSock[%d] disconnected!\n",i);
					close(clntSock[i]);
					for(j=i;j<clntNum-1;j++) clntSock[j] = clntSock[j+1];
					clntNum--; i--; continue;
				}

				// broadcast message
				for(j=0;j<clntNum;j++)	send(clntSock[j],buffer,recvSize,0);
				printf("%s\n",buffer);
			}
		}
	}
}

