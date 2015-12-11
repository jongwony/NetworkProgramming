#include<stdio.h>
#include<sys/socket.h>
#include<arpa/inet.h>
#include<stdlib.h>
#include<string.h>
#include<unistd.h>

void gameStart(int clntSock);

int main(void){
	struct sockaddr_in servAddr;
	unsigned short servPort=20000;

	int clntSock;
	int connSock;

//	char quit;
	
	clntSock = socket(PF_INET, SOCK_STREAM, IPPROTO_TCP);
	if(clntSock<0){
		perror("socket() failed");
	}

	memset(&servAddr,0,sizeof(servAddr));
	servAddr.sin_family = AF_INET;
	servAddr.sin_addr.s_addr = inet_addr("127.0.0.1");
	servAddr.sin_port = htons(servPort);
	
	connSock = connect(clntSock, (struct sockaddr *)&servAddr,
			sizeof(servAddr));
	if(connSock<0){
		perror("connect() failed");
	}
	printf("server-client connected!\n");

	gameStart(clntSock);
		
	/*do{
		scanf("%c",&quit);
		printf("quit code->%c\n",quit);
	}while(quit!='q');*/
	return 0;
}	
