#include<stdio.h>
#include<sys/socket.h>
#include<sys/time.h>
#include<sys/types.h>
#include<arpa/inet.h>
#include<string.h>
#include<unistd.h>

int main(void){
	int clntSock;
	struct sockaddr_in inputAddr;
	int maxfd, recvSize;
	char buffer[256],input[256];
	fd_set readfd;

	// open socket
	clntSock = socket(PF_INET, SOCK_STREAM, IPPROTO_TCP);
	if(clntSock<0)	perror("socket() failed");

	// client address
	inputAddr.sin_family = AF_INET;
	inputAddr.sin_addr.s_addr = inet_addr("127.0.0.1");
	inputAddr.sin_port = htons(20000);

	// connect socket
	connect(clntSock,(struct sockaddr*)&inputAddr,sizeof(inputAddr));
	printf("connect success!\n");

	// fd set
	maxfd = clntSock+1;
	FD_ZERO(&readfd);
	
	while(1){		
		FD_SET(0,&readfd);		// fd 0 : input file descriptor
		FD_SET(clntSock,&readfd);

		// select
		select(maxfd,&readfd,NULL,NULL,NULL);

		// keyboard input
		if(FD_ISSET(0,&readfd)){
			scanf("%s",input);
			send(clntSock,input,256,0);
			printf("send!\n");
		}

		// client socket selected
		if(FD_ISSET(clntSock,&readfd)){
			if((recvSize=recv(clntSock,buffer,256,0))>0)	printf("%s\n",buffer);
		}
	}
	close(clntSock);
	return 0;
}
