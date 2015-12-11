#include<stdio.h>
#include<stdio_ext.h>	//__fpurge();
#include<sys/socket.h>
#include<unistd.h>

#define FLAG0 "Your Turn\n"
#define FLAG1 "Enemy Turn\n"

int isVictory(int *state);

void gameStart(int clntSock){
	int flag;
	int clntFlag=0;
	int isGameset=0;
	int state[9]={0,};
	int coord;
	int i;

	while(1){
		// sock0 send, sock1 recv
		recv(clntSock,&flag,4,0);

		if(flag==0){
			printf(FLAG0);
			// put coordinate
			while(1){
				printf("select num(1~9):");
				__fpurge(stdin);
				scanf("%d",&coord);
				if(!state[coord-1])	break;
				else	printf("already exist!\n");
			}
			send(clntSock,&coord,4,0);
		}else{
			printf(FLAG1);
			printf("\n");
			recv(clntSock,&coord,4,0);
			printf("clntSock = %d\n",clntSock);
		}
		
		// table config
		clntFlag = !clntFlag;
		if(clntFlag)	state[coord-1]=1;
		else	state[coord-1]=2;

		for(i=0;i<9;i++){
			printf("%d\t",state[i]);
			if(i%3==2){
				printf("\n");
			}
		}

		// isVictory(Bit Mask)
		isGameset = isVictory(state);

		// isGameset?
		if(flag==0){
			send(clntSock,&isGameset,4,0);
			if(isGameset){
				printf("VICTORY!\n");
				break;
			}
		}else{
			if(isGameset){
				printf("DEFEAT!\n");
				break;
			}
		}
	}
	close(clntSock);
}
