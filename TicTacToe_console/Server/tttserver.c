#include<stdio.h>
#include<stdlib.h>
#include<arpa/inet.h>
#include<string.h>
#include<unistd.h>
#include<sys/types.h>

void gameHandle(int *clntSock);

int main(void){
	struct sockaddr_in servAddr;
	struct sockaddr_in clntAddr;
	unsigned short servPort=20000;
	unsigned int clntLen;

	int servSock;
	int servBind;
	int servListen;
	int clntSock[2];

	pid_t pid;

	// open socket
	servSock = socket(PF_INET, SOCK_STREAM, IPPROTO_TCP);
	if(servSock<0){
		perror("socket() failed");
	}

	// binding server address, port
	memset(&servAddr, 0, sizeof(servAddr));
	servAddr.sin_family = AF_INET;
	servAddr.sin_addr.s_addr = htonl(INADDR_ANY);
	servAddr.sin_port = htons(servPort);
	servBind = bind(servSock, (struct sockaddr *)&servAddr,
			sizeof(servAddr));
	if(servBind<0){
		perror("bind() failed");
	}

	//listen client
	servListen = listen(servSock, 1);
	if(servListen<0){
		perror("listen() failed");
	}

	printf("Server Start!\n");
	while(1){
		printf("loop Start\n");
		// add client socket for connect client
		clntLen = sizeof(clntAddr);
		clntSock[0] = accept(servSock,(struct sockaddr *)&clntAddr,
				&clntLen);
		if(clntSock[0]<0){
			perror("accept() failed");
		}
		printf("clntSock[0] accepted!\n");
		
		clntSock[1] = accept(servSock,(struct sockaddr *)&clntAddr,
				&clntLen);
		if(clntSock[1]<0){
			perror("accept() failed");
		}
		printf("clntSock[1] accepted!\n");
		
		
		// Create process
		pid = fork();

		switch(pid){
			case -1:
				perror("process() error");
				return -1;
			case 0:
				// Game start!
				gameHandle(clntSock);
				exit(0);
			default:
				printf("Room Created!\n");
				break;
		}

	}
	return 0;
}
