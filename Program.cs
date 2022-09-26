using Nethereum.Web3;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts.CQS;
using Nethereum.Util;
using Nethereum.Web3.Accounts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Contracts;
using Nethereum.Contracts.Extensions;
using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.HdWallet;

namespace NethereumSample
{
    class Program
    {
        public class NFTCollection : ContractDeploymentMessage
        {
            public static string BYTECODE = "0x608060405234801561001057600080fd5b5060405161027d38038061027d83398181016040528101906100329190610054565b80600081905550506100a7565b60008151905061004e81610090565b92915050565b60006020828403121561006a5761006961008b565b5b60006100788482850161003f565b91505092915050565b6000819050919050565b600080fd5b61009981610081565b81146100a457600080fd5b50565b6101c7806100b66000396000f3fe608060405234801561001057600080fd5b506004361061002b5760003560e01c8063c6888fa114610030575b600080fd5b61004a6004803603810190610045919061008b565b610060565b60405161005791906100c7565b60405180910390f35b600080548261006f91906100e2565b9050919050565b6000813590506100858161017a565b92915050565b6000602082840312156100a1576100a0610175565b5b60006100af84828501610076565b91505092915050565b6100c18161013c565b82525050565b60006020820190506100dc60008301846100b8565b92915050565b60006100ed8261013c565b91506100f88361013c565b9250817fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff048311821515161561013157610130610146565b5b828202905092915050565b6000819050919050565b7f4e487b7100000000000000000000000000000000000000000000000000000000600052601160045260246000fd5b600080fd5b6101838161013c565b811461018e57600080fd5b5056fea26469706673582212203661710ba712f3b7396b148ce739251df683fb33453533324091f8e26e3e1fff64736f6c63430008070033";
            public NFTCollection() : base(BYTECODE)
            {

            }

            [Parameter("uint256", "totalSupply")]
            public BigInteger TotalSupply { get; set; }
        }

        static async Task Main(string[] args)
        {
            //GetAccountBalance().Wait();
            //Console.ReadLine();

            //  Instantiating Web3 and the Account

            // To create an instance of web3 we first provide the url of our testchain and the private key of our account. 
            // When providing an Account instantiated with a  private key all our transactions will be signed “offline” by Nethereum.

            var privateKey = "0x7580e7fb49df1c861f0050fae31c2224c6aba908e116b8da44ee8cd927b990b0";
            var chainId = 444444444500; //Nethereum test chain, chainId
            var account = new Account(privateKey, chainId);
            Console.WriteLine("Our account: " + account.Address);
            //Now let's create an instance of Web3 using our account pointing to our nethereum testchain
            var web3 = new Web3(account, "http://testchain.nethereum.com:8545");
            // web3.TransactionManager.UseLegacyAsDefault = true; //Using legacy option instead of 1559

            //  Deploying the Contract
            // The next step is to deploy our Standard Token ERC20 smart contract, in this scenario the total supply (number of tokens) is going to be 100,000.
            // First we create an instance of the StandardTokenDeployment with the TotalSupply amount.

            var deploymentMessage = new NFTCollection
            {
                TotalSupply = 100000
            };

            // Then we create a deployment handler using our contract deployment definition and simply deploy the contract using the deployment message. We are auto estimating the gas, getting the latest gas price and nonce so nothing else is set anything on the deployment message.
            // Finally, we wait for the deployment transaction to be mined, and retrieve the contract address of the new contract from the receipt.

            var deploymentHandler = web3.Eth.GetContractDeploymentHandler<NFTCollection>();

            var transactionReceipt = await deploymentHandler.SendRequestAndWaitForReceiptAsync(deploymentMessage);

            var contractAddress = transactionReceipt.ContractAddress;

            Console.WriteLine("Deployed Contract address is: " + contractAddress);
        }

        static async Task SendEthers()
        {
            // This samples shows how to create an HD Wallet using BIP32 standard in Ethereum.
            // For simpler context, this allows you to recover your accounts and private keys created with a seed set of words
            // For example Metamask uses 12 words
            // 
            //Nethereum uses internally NBitcoin to derive the private and public keys, for more information on BIP32 check
            //https://programmingblockchain.gitbook.io/programmingblockchain/key_generation/bip_32

            //Initiating a HD Wallet requires a list of words and an optional password to add further entropy (randomness)

            var words = "ripple scissors kick mammal hire column oak again sun offer wealth tomorrow wagon turn fatal";
            //Note: do not confuse the password with your Metamask password, Metamask password is used to secure the storage
            var password = "password";
            var wallet = new Wallet(words, password);

            // An HD Wallet is deterministic, it will derive the same number of addresses 
            // given the same seed (wordlist + optional password).

            // All the created accounts can be loaded in a Web3 instance and used as any other account, 
            // we can for instance check the balance of one of them:

            var chainId = 444444444500;
            var account = new Wallet(words, password).GetAccount(0, chainId);
            Console.WriteLine("The account address is: " + account.Address);

            var web3 = new Web3(account, "http://testchain.nethereum.com:8545");
            //we connect to the Nethereum testchain which has already the account preconfigured with some Ether balance.
            var balance = await web3.Eth.GetBalance.SendRequestAsync(account.Address);
            Console.WriteLine("The account balance is: " + balance.Value);

            //Or transfer some Ether, as the account already has the private key required to sign the transactions.

            var toAddress = "0x13f022d72158410433cbd66f5dd8bf6d2d129924";
            var transactionReceipt = await web3.Eth.GetEtherTransferService()
                .TransferEtherAndWaitForReceiptAsync(toAddress, 2.11m, 2);
            Console.WriteLine($"Transaction {transactionReceipt.TransactionHash} for amount of 2.11 Ether completed");
        }
        //static async Task SendEther()
        //{
        //    var privateKey = "0xb5b1870957d373ef0eeffecc6e4812c0fd08f554b37b233526acc331bf1544f7";
        //    var account = new Account(privateKey);

        //    var web3 = new Web3(account);

        //    var toAddress = "0x13f022d72158410433cbd66f5dd8bf6d2d129924";

        //    //var transaction = await web3.Eth.GetEtherTransferService()
        //    //    .TransferEtherAndWaitForReceiptAsync(toAddress, 1.11m);

        //    //var transaction = await web3.Eth.GetEtherTransferService()
        //    //    .TransferEtherAndWaitForReceiptAsync(toAddress, 1.11m, 2);

        //    var transaction = web3.Eth.GetEtherTransferService()
        //        .TransferEtherAndWaitForReceiptAsync(toAddress, 1.11m, 2, new BigInteger(25000));

        //}

        static async Task GetAccountBalance()
        {
            var web3 = new Web3("https://mainnet.infura.io/v3/e27e450636c94111a358c80d3997ee9d");
            var balance = await web3.Eth.GetBalance.SendRequestAsync("0xde0b295669a9fd93d5f28d9ec85e40f4cb697bae");
            Console.WriteLine($"Balance in Wei: {balance.Value}");

            var etherAmount = Web3.Convert.FromWei(balance.Value);
            Console.WriteLine($"Balance in Ether: {etherAmount}");
        }
    }
}